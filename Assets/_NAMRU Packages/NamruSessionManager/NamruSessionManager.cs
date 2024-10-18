using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using BM_Enums;
using UDPConnector;

namespace NAMRU_data
{
	public enum TrialResultsMode
	{
		ToTextFile,
		ToFennec,
	}

	public enum TrialResultsFormat
	{
		none,
		csv,
	}

	public class NamruSessionManager : MonoBehaviour
	{
		public static NamruSessionManager Instance;
		/// <summary>
		/// This is a useful truth variable for only doing certain operations if we're before or after the first singleton load of this class.
		/// </summary>
		private bool amPastFirstLoad = false;
		/// <summary>
		/// This is a useful truth variable for only doing certain operations if we're before or after the first singleton load of this class.
		/// </summary>
		public bool AmPastFirstLoad => amPastFirstLoad;

		[Header("[----------- FILE ----------]")]
		[SerializeField] private string participantID = "Unnamed"; //"Unnamed" is the default that will persist if not explicitely set to something different.
		public string ParticipantID => participantID;

		/// <summary>
		/// String identifying the session in the full 
		/// </summary>
		[SerializeField] private string sessionString;

		/// <summary>
		/// This will be the path to the folder will all namru-related files will be.  This will include the changelog file, any ini files, and any session output I want to write.
		/// Will be "[Root data folder]/Namru Data/"
		/// </summary>
		[SerializeField] private string dirPath_NamruDirectory;
		public string DirPath_NamruDirectory => dirPath_NamruDirectory;

		/// <summary>
		/// Points to a directory that houses all trial data for all participants. Will be "[Root data folder]/Namru Data/TrialResults/"
		/// </summary>
		[SerializeField] private string DirPath_TrialResultsDirectory;

		/// <summary>
		/// Points to a directory designated for the current session. Will be "[Root data folder]/Namru Data/TrialResults/[Participant ID]"
		/// </summary>
		[SerializeField] private string DirPath_CurrentSessionDirectory;

		[Header("[----------- INI ----------]")]
		[SerializeField] string filePath_ini;
		[SerializeField, Tooltip("These will ideally be read in from a succesful ini read, but also should be populated with default values from the inspector in case the read isn't succesful")] 
		public string[] IniValues;

		/// <summary>
		/// Data from the trial results of the current participant for the current session. This is the "final-most" filepath that exists. Ultimately where the session data will log to. 
		/// "will be {ParticipantID}_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_session"
		/// </summary>
		[SerializeField] private string filePath_sessionTrialResults;
		/// <summary>
		/// Data for the logs from the current session.
		/// </summary>
		[SerializeField] private string filePath_log;

		[Header("[----------- WRITERS ----------]")]
		private StreamWriter streamWriter_log;
		private StreamWriter streamWriter_trialResults;

		[Header("[----------- SESSION DATA ----------]")]
		[Tooltip("Makes the format for the csv file headers for the output for this session")] 
		public List<string> DataHeaders;
		[SerializeField] TrialResultsMode trialResultsMode;
		[SerializeField] TrialResultsFormat trialResultsFormat;

		[Header("[----------- TRUTH ----------]")]
		[Tooltip("Allows independent control over whether this manager will output trial results for the current session")]
		public bool AmOutputtingTrialResults = true;
		[Tooltip("Allows independent control over whether this manager will output logs for the current session")]
		public bool AmDoingLog = true;
		[Tooltip("Allows independent control over whether this manager will bother with ini checking/setup/reading for the current session")]
		public bool AmUsingIni = true;
		[Tooltip("Allows independent control over whether this manager will start a UDP listener for the current session")]
		public bool AmListeningViaUDP = true;
		[Tooltip("Allows independent control over whether this manager will start a UDPFennec instance for the current session")]
		public bool AmFennecSending = true;

		[Header("[----------- UDP LISTENING ----------]")]
		public int port = 11000;
		UdpClient udpClient;
		IPEndPoint ipEndPt;
		private string receiveString;
		public string ReceiveString => receiveString;
		private byte[] receive_byte_array;
		private Thread listeningThread;
		private bool continueListeningThread = false;

		[Header("[----------- FENNEC SENDING ----------]")]
		private UDPFennec fennecSender;
		public string FennecIPAddressString = "10.10.101.43"; //default ip: "127.0.0.1"

        //[Header("[----------- FLAGS ----------]")]
        /// <summary>
        /// This is a flag that gets set when you decide to close the app. This is a fail-safe so it doesn't try to write to the changelog after a certain point in the process of closing.
        /// </summary>
        bool flag_writersShouldBeDisposed = false;

		/// <summary>
		/// This is a flag that gets set when attempt is made to load the ini file with TryToLoadIni(). Note: It only get set to true if the ini file is present in the correct location, and is 
		/// of the correct length. It doesn't validate the individual values. You should do this with an external script you make.
		/// </summary>
		private bool flag_haveSuccesfullyReadAndParsedValidIniFile = false;
		/// <summary>
		/// This is a flag that gets set when attempt is made to load the ini file with TryToLoadIni(). Note: It only get set to true if the ini file is present in the correct location, and is 
		/// of the correct length. It doesn't validate the individual values. You should do this with an external script you make.
		/// </summary>
		public bool Flag_HaveSuccesfullyReadAndParsedValidIniFile => flag_haveSuccesfullyReadAndParsedValidIniFile;

		private bool flag_haveSuccesfullyFoundOrCreatedNamruiDirectory = false;
		private bool flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory = false;
		private bool flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory = false;

		private bool flag_haveSuccesfullyCreatedSessionFile = false;
		private bool flag_haveSuccesfullyFoundOrCreatedLogFile = false;


		private void Awake()
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.Awake()" );
			DontDestroyOnLoad( this );

			if ( Instance == null )
			{
				NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.{nameof(Instance)} was null. Setting singleton reference to this..." );

				Instance = this;

			}
			else if ( Instance != this )
			{
				NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.{nameof(Instance)} was NOT null. Destroying this..." );

				Destroy( gameObject );
			}

			NAMRU_Debug.Log ( $"{gameObject.name} > {nameof(NamruSessionManager)}.Awake() end" );

		}

		private void Start()
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.Start()" );

			amPastFirstLoad = true;

			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.Start() end" );

		}

		/// <summary>
		/// This initializes only the necessary path strings, such as the root namru directory path, the trial results path, the ini filepath and the log filepath. This is for all 
		/// the paths that are NOT specific to the session or individual participant.
		/// </summary>
		public void InitializePreliminaryPaths()
		{
			if ( Application.isEditor )
			{
				dirPath_NamruDirectory = "NAMRU Data"; //will be one level outside of Assets
			}
			else
			{
				dirPath_NamruDirectory = Path.Combine( Application.dataPath, "NAMRU Data" ); //Will be inside a folder inside the build folder with "_Data" at the end.
			}

			DirPath_TrialResultsDirectory = Path.Combine( dirPath_NamruDirectory, "TrialResults" );
			filePath_ini = Path.Combine( dirPath_NamruDirectory, "ini.txt" );
			filePath_log = Path.Combine( dirPath_NamruDirectory, "log.txt" );
		}

		public void TryToLoadIni()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToLoadIni)}()" );

			if ( File.Exists(filePath_ini) )
			{
				NAMRU_Debug.Log($"Found ini file at default path. Attempting to load ini file...");
				string[] rslts;

				try
				{
					rslts = File.ReadAllLines(filePath_ini);

				}
				catch ( Exception e )
				{
					NAMRU_Debug.LogError( $"Exception caught while attempting to read ini file. Exception says: '{e}" );
					return;
				}

				if ( rslts.Length <= 0 )
				{
					NAMRU_Debug.LogError( $"ini file only returned '{rslts.Length}' lines. Can't read ini file..." );
					return;
				}
				else if( rslts.Length < IniValues.Length )
				{
					NAMRU_Debug.LogError( $"ini file only returned '{rslts.Length}' lines. This is different from the amount of ini values expected. Using default settings..." );
					return;
				}
				else
				{
					IniValues = rslts;
					flag_haveSuccesfullyReadAndParsedValidIniFile = true;
				}
			}
			else
			{
				NAMRU_Debug.Log( $"ini file at path: '{filePath_ini}' does not exist. Using default settings.", LogDestination.All );
			}

			return;
		}
		
		public void TryToStartLogFileWriter()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToStartLogFileWriter)}()" );
			NAMRU_Debug.Log( $"trying to create log at filepath: '{filePath_log}'" );
			try
			{
				streamWriter_log = new StreamWriter( filePath_log );
				streamWriter_log.WriteLine( $"Created log at: '{DateTime.Now}'" );

				//File.WriteAllText( FilePath_log, $"Created log at: '{System.DateTime.Now}'" );
				flag_haveSuccesfullyFoundOrCreatedLogFile = true;

			}
			catch ( Exception e )
			{
				NAMRU_Debug.LogError(e.ToString());
				return;
			}
		}

		/// <summary>
		/// Takes in a path string, checks if a directory exists at it's path, and if not, tries to create a directory.
		/// </summary>
		/// <param name="dirPath_passed"></param>
		/// <returns>true if it is succesful at either finding or creating a directory at the supplied path. False if not succesful at one of these.</returns>
		private bool TryToFindOrCreateDirectory( string dirPath_passed )
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToFindOrCreateDirectory)}({nameof(dirPath_passed)}: '{dirPath_passed}')" );

			if ( Directory.Exists(dirPath_NamruDirectory) )
			{
				NAMRU_Debug.Log( $"DID find {nameof(dirPath_NamruDirectory)}." );
				return true;
			}
			else
			{
				NAMRU_Debug.Log( $"Did NOT find '{nameof(dirPath_NamruDirectory)}' creating directory..." );

				try
				{
					Directory.CreateDirectory( dirPath_NamruDirectory );
					NAMRU_Debug.Log( $"Succesfully created directory at: '{dirPath_NamruDirectory}'" );
				}
				catch ( Exception e )
				{
					NAMRU_Debug.LogError( $"Caught exception attempting to create directory. Exception says: {e}" );
					return false;
				}
			}

			return true;
		}

		public void TryToFindOrCreateNamruDirectory()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToFindOrCreateNamruDirectory)}({nameof(dirPath_NamruDirectory)}: '{dirPath_NamruDirectory}')" );

			flag_haveSuccesfullyFoundOrCreatedNamruiDirectory = TryToFindOrCreateDirectory( dirPath_NamruDirectory );
		}

		public void TryToFindOrCreateTrialResultsDirectory()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToFindOrCreateTrialResultsDirectory)}({nameof(DirPath_TrialResultsDirectory)}: '{DirPath_TrialResultsDirectory}')" );

			flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory = TryToFindOrCreateDirectory( DirPath_TrialResultsDirectory );
		}

		public void TryToFindOrCreateCurrentSessionDirectory()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(TryToFindOrCreateCurrentSessionDirectory)}({nameof(DirPath_CurrentSessionDirectory)}: '{DirPath_CurrentSessionDirectory}')" );

			flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory = TryToFindOrCreateDirectory( DirPath_CurrentSessionDirectory );
		}

		public void SetParticipantId_action( string s )
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(SetParticipantId_action)}({s})" );

			participantID = s;

			if ( string.IsNullOrEmpty(participantID) )
			{
				NAMRU_Debug.LogError( $"supplied participant id was empty. Cannot calculate a session filepath without a valid participant ID. Returning early..." );
				return;
			}

			DirPath_CurrentSessionDirectory = Path.Combine( DirPath_TrialResultsDirectory, participantID );
			bool sessDirAlreadyExisted = File.Exists( DirPath_CurrentSessionDirectory );
			TryToFindOrCreateCurrentSessionDirectory();

			sessionString = $"{participantID}_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_session";
			int sessionNumb = 1;

			if ( sessDirAlreadyExisted )
			{
				NAMRU_Debug.Log( $"Directory '{DirPath_CurrentSessionDirectory}' already existed. Using this directory for trial data. Now attempting to generate a unique session string..." );

				bool foundUniqueSessionNumber = false;
				while ( !foundUniqueSessionNumber )
				{
					if ( File.Exists(Path.Combine(DirPath_CurrentSessionDirectory, $"{sessionString}{sessionNumb}.txt")) )
					{
						sessionNumb++;
					}
					else
					{
						foundUniqueSessionNumber = true;
						sessionString += sessionNumb;
						filePath_sessionTrialResults = Path.Combine( DirPath_CurrentSessionDirectory, sessionString );
						NAMRU_Debug.Log( $"Found unique filepath for session at: '{filePath_sessionTrialResults}'" );
					}

					if ( sessionNumb > 100 )
					{
						NAMRU_Debug.LogError($"Found over 100 sessions for this user. Assuming infinite loop. Breaking early...");
						return;
					}
				}
			}
			else
			{
				NAMRU_Debug.Log($"Directory '{DirPath_CurrentSessionDirectory}' did NOT already exist. Making this directory for trial data...");
				sessionString += sessionNumb;
				filePath_sessionTrialResults = Path.Combine( DirPath_CurrentSessionDirectory, sessionString );
			}

			NAMRU_Debug.Log( $"Succesfully created {nameof(filePath_sessionTrialResults)} of: '{filePath_sessionTrialResults}. Now attempting to create this file..." );
			try
			{
				File.Create( filePath_sessionTrialResults + ".txt" );
			}
			catch ( Exception e )
			{
				NAMRU_Debug.LogError( $"Caught exception trying to create session file. Exception says:" );
				NAMRU_Debug.Log( e.ToString() );
				return;
			}

			NAMRU_Debug.Log( "succesfully created session file" );
			flag_haveSuccesfullyCreatedSessionFile = true;

			NAMRU_Debug.Log($"{nameof(NamruSessionManager)}.{nameof(SetParticipantId_action)}() end");

		}

		public void WriteToLogFile( string msg )
		{
			if( AmDoingLog && flag_haveSuccesfullyFoundOrCreatedLogFile && !flag_writersShouldBeDisposed && streamWriter_log != null )
			{
				streamWriter_log.WriteLine( msg );
			}
		}

		/// <summary>
		/// Use this to write trial event data in the moment.
		/// </summary>
		/// <param name="s"></param>
		public void WriteToTrialResults( string s )
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(WriteToTrialResults)}('{s}')" );

			if ( AmOutputtingTrialResults && flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory )
			{
				if( trialResultsMode == TrialResultsMode.ToFennec )
				{

				}
			}
		}

		public bool StartUDPListening()
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.{nameof(StartUDPListening)}()", BM_Enums.LogDestination.none );
			// UDP init stuff-----------------
			int attemptCount = 0;
			bool isConnected = false;
			while ( !isConnected )
			{
				try
				{
					udpClient = new UdpClient(port);
					ipEndPt = new IPEndPoint(IPAddress.Any, 0);

					isConnected = true;
				}
				catch ( SocketException e )
				{
					attemptCount++;

					if ( attemptCount == 10 )
					{
						NAMRU_Debug.LogWarning( $"udpclient exception says: {e}" );
						return false;
					}
				}
			}

			NAMRU_Debug.Log( $"UDP client initialization did not error. Now initializing listener thread..." );
			listeningThread = new Thread( new ThreadStart(ListenForUDPPacket_action) );
			listeningThread.IsBackground = true;
			continueListeningThread = true;
			listeningThread.Start();

			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(NamruSessionManager)}.{nameof(StartUDPListening)}() end. Returning true...", BM_Enums.LogDestination.none );
			return true;
		}

		public void ListenForUDPPacket_action()
		{
			while ( continueListeningThread )
			{
				try
				{
					receive_byte_array = udpClient.Receive( ref ipEndPt ); //I believe that the event-like behavior of this is because this line blocks execution until it recieves something
					receiveString = Encoding.ASCII.GetString( receive_byte_array, 0, receive_byte_array.Length );
	
					if( !string.IsNullOrEmpty(receiveString) )
					{
						GameManager.Instance.RecieveUDPString_threadSafe( receiveString );
					}
				}
				catch (SocketException)
				{
					NAMRU_Debug.LogWarning( $"socket exception caught", true );
				}
				catch ( Exception e )
				{
					//I don't think debug logging in Unity is thread safe...
					
					NAMRU_Debug.LogError( $"Got exception of type: '{e.GetType()}' attempting to listen to UDP. gamemanager null?: '{GameManager.Instance == null}' Exception says:", true );
					NAMRU_Debug.LogError( e.ToString(), true );
				}
			}
		}

		public void StartFennec()
		{
			NAMRU_Debug.Log($"{nameof(NamruSessionManager)}.{nameof(StartFennec)}()");

			try
			{
				fennecSender = new UDPFennec( FennecIPAddressString );

			}
			catch ( Exception e )
			{

				NAMRU_Debug.LogError( $"Caught exception of type: '{e.GetType()}' trying to initialize a fennec sender. Exception says: " + Environment.NewLine +
					e.ToString() );
			}

			NAMRU_Debug.Log($"{nameof(NamruSessionManager)}.{nameof(StartFennec)} end");
		}

		public void SendToFennec( string name, double value, bool sendData = false )
		{
			//NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(SendToFennec)}('{name}', '{value}')" );
			fennecSender.AddData( name, value );

			if( sendData )
			{
				fennecSender.SendData();
			}

			//NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(SendToFennec)}()" );
		}


		[ContextMenu("z call CloseMe()")]
		public void CloseMe()
		{
			NAMRU_Debug.Log($"{nameof(NamruSessionManager)}.{nameof(CloseMe)}()");
			CloseUDPConnection();

			DisposeAllWriters();
			//NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(CloseMe)}() end" );

		}

		[ContextMenu("z call DisposeAllWriters()")]
		public void DisposeAllWriters()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(DisposeAllWriters)}()", LogDestination.console );
			int spot = 0;

			flag_writersShouldBeDisposed = true;
			spot = 1;

			try
			{
				spot = 2;
				if ( streamWriter_log != null )
				{
					NAMRU_Debug.Log( $"{nameof(streamWriter_log)} was not null. Disposing...", LogDestination.console );
					streamWriter_log.Dispose(); //explicitely calling this because I'm keeping a persistant logwriter variable, vs using it in an auto-managed using statement.
					streamWriter_log = null; //I think the docs say if you're going to call dispose, you need to do this to truly release it.
				}
				else
				{
					NAMRU_Debug.Log($"{nameof(streamWriter_log)} was null...", LogDestination.console );
				}
				spot = 3;

				if( streamWriter_trialResults != null )
				{
					NAMRU_Debug.Log($"{nameof(streamWriter_trialResults)} was not null. Disposing...", LogDestination.console );
					streamWriter_trialResults.Dispose(); //explicitely calling this because I'm keeping a persistant logwriter variable, vs using it in an auto-managed using statement.
					streamWriter_trialResults = null; //I think the docs say if you're going to call dispose, you need to do this to truly release it.
				}
				else
				{
					NAMRU_Debug.Log($"{nameof(streamWriter_trialResults)} was null...", LogDestination.console );
				}
				spot = 4;
			}
			catch ( Exception e )
			{
				NAMRU_Debug.LogError( $"Got exceptipon attempting to dispose writers after spot: '{spot}'. Exception says:" );
				NAMRU_Debug.LogError(e.ToString());
			}

			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(DisposeAllWriters)}() end", LogDestination.console );

		}

		public void CloseUDPConnection()
		{
			NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(CloseUDPConnection)}()" );
			continueListeningThread = false;

			if( udpClient != null )
			{
				try
				{
					udpClient.Close();
				}
				catch( SocketException)
				{
					NAMRU_Debug.LogWarning( $"socket exception caught" );
				}
				catch ( Exception e )
				{
					NAMRU_Debug.LogError( "Got exceptipon attempting to close udp listener. Exception says:" );
					NAMRU_Debug.Log( e.ToString() );
				}
			}
			else
			{
				NAMRU_Debug.Log( $"Was going to close {nameof(udpClient)}, but found it was already null..." );
			}

			NAMRU_Debug.Log($"{nameof(NamruSessionManager)}.{nameof(CloseUDPConnection)}() end");

		}

        private void OnDestroy()
        {
			CloseMe();
        }
    }
}
