using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LFP
{
	public static class LFP_Utilities
	{
		static char[] delimiterChars = { '/', '\\' };

		public static string[] GetDelimitedPath( string s )
		{
			return s.Split( delimiterChars );
		}

		public static string GetBeginningInPathString( string s )
		{
			string[] s_split = s.Split(delimiterChars);
			return s_split[0];
		}

		public static string GetEndInPathString( string s )
		{
			string[] s_split = s.Split( delimiterChars );
			return s_split[ s_split.Length - 1 ];
		}

		public static string GetPresentableAbbreviatedDirString( string[] delimitedArray )
		{
			if ( delimitedArray == null || delimitedArray.Length <= 0 )
			{
				return string.Empty;
			}
			else if ( delimitedArray.Length < 3 )
			{
				string s = "";
				for ( int i = 0; i < delimitedArray.Length; i++ )
				{
					s += delimitedArray[i];
					if (i < delimitedArray.Length - 1)
					{
						s += "/";
					}
				}

				return s;
			}
			else
			{
				return delimitedArray[0] + "/.../" + delimitedArray[ delimitedArray.Length - 1 ];
			}
		}
	}

	/// <summary>
	/// Struct that can be used to hold data and operations for a set directory
	/// </summary>
	[System.Serializable]
	public struct DirPathInfo
	{
		/// <summary>
		/// Complete string for the directory path pointed to by this object.
		/// </summary>
		public string DirectoryString_full;

		public string PreviousDirectoryString_full;

		/// <summary>
		/// Array of all the delimited strings in this object's directory string.
		/// </summary>
		public string[] DelimitedDirectoryPathStrings;

		/// <summary>
		/// List containing the full paths of all subdirectories in the folder pointed to by this object
		/// </summary>
		public List<string> FullStrings_ContainedSubDirectories;
		/// <summary>
		/// List containing only the folder names of all the subdirectories in the folder pointed to by this object.
		/// </summary>
		public List<string> EndStrings_ContainedSubDirectories;

		public List<string> FullStrings_ContainedFilePaths;
		public List<string> EndStrings_ContainedFilePaths;


		public void SetMeViaDirectoryPath ( string dirString_passed )
		{
			DirectoryString_full = dirString_passed;

			DelimitedDirectoryPathStrings = LFP_Utilities.GetDelimitedPath( dirString_passed );

			PreviousDirectoryString_full = dirString_passed.Substring( 0, dirString_passed.Length - 
				DelimitedDirectoryPathStrings[DelimitedDirectoryPathStrings.Length-1].Length - 1 );

			FullStrings_ContainedSubDirectories = new List<string>( Directory.EnumerateDirectories(DirectoryString_full) );
			EndStrings_ContainedSubDirectories = new List<string>();

			for ( int i = 0; i < FullStrings_ContainedSubDirectories.Count; i++)
			{
				EndStrings_ContainedSubDirectories.Add( LFP_Utilities.GetEndInPathString(FullStrings_ContainedSubDirectories[i]) );
			}

			FullStrings_ContainedFilePaths = new List<string>( Directory.EnumerateFiles(DirectoryString_full) );
			EndStrings_ContainedFilePaths = new List<string>();
			for ( int i = 0; i < FullStrings_ContainedFilePaths.Count; i++ )
			{
				EndStrings_ContainedFilePaths.Add( LFP_Utilities.GetEndInPathString(FullStrings_ContainedFilePaths[i]) );
			}
		}

		public string GetEndOfDirString()
		{
			if( DelimitedDirectoryPathStrings == null || DelimitedDirectoryPathStrings.Length <= 0 )
			{
				return string.Empty;
			}
			else
			{
				return DelimitedDirectoryPathStrings[ DelimitedDirectoryPathStrings.Length - 1 ];
			}
		}

		public string GetBeginningOfDirString()
		{
			if ( DelimitedDirectoryPathStrings == null || DelimitedDirectoryPathStrings.Length <= 0 )
			{
				return string.Empty;
			}
			else
			{
				return DelimitedDirectoryPathStrings[0];
			}
		}

		public string GetPresentableAbbreviatedDirString()
		{
			return LFP_Utilities.GetPresentableAbbreviatedDirString( DelimitedDirectoryPathStrings );
		}
	}
}

