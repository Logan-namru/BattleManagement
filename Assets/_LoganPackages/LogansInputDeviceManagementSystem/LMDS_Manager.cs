using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LogansMultiDeviceSystem
{
    public class LMDS_Manager : MonoBehaviour
    {
        public static LMDS_Manager Instance;

        private LMDS_Device[] connectedDevices;
        public LMDS_Device[] ConnectedDevices => connectedDevices;
        private string[] connectedDeviceNames;

        public bool HasConnectedDevices
        {
            get
            {
                return connectedDevices != null && connectedDevices.Length > 0;
            }
        }

        private void Awake()
        {
            //DontDestroyOnLoad(this); //I don't think this is necessary because I don't think I'm going 
            //to put this manager in the battle scene, only the start scene.

            if ( Instance == null )
            {
                Instance = this;
                GetConnectedDevices();
            }
            else
            {
                Destroy( gameObject );
            }
        }

        void Start()
        {

        }

        public void GetConnectedDevices()
        {
            connectedDeviceNames = Input.GetJoystickNames();
            connectedDevices = new LMDS_Device[connectedDeviceNames.Length ];

            for( int i = 0; i < connectedDeviceNames.Length; i++ )
            {
                connectedDevices[i] = new LMDS_Device( i, connectedDeviceNames[i] );
            }
        }

        public bool FindDeviceViaName( string name, ref LMDS_Device foundDevice )
        {
            if ( connectedDevices != null && connectedDevices.Length > 0 )
            {
                for ( int i = 0; i < connectedDevices.Length; i++ )
                {
                    if( connectedDevices[i].DeviceName == name )
                    {
                        foundDevice = connectedDevices[i];
                        return true;
                    }
                }
            }

            foundDevice = LMDS_Device.none;
            return false;
        }

        public bool FindDeviceViaPlayerPref( string playerPrefKey, ref LMDS_Device foundDevice )
        {
            string Name_savedDevice = PlayerPrefs.GetString( playerPrefKey );

            return FindDeviceViaName( Name_savedDevice, ref foundDevice );
        }


        public void PopulateDropdown( TMP_Dropdown dropdown )
        {
            dropdown.ClearOptions();
            dropdown.AddOptions( connectedDeviceNames.ToList() );
        }

        public string GetDebugString()
        {
            string dbgString = string.Empty;
            dbgString += $"Fetched devices: ";
            if ( connectedDeviceNames != null && connectedDeviceNames.Length > 0 )
            {
                for ( int i = 0; i < connectedDeviceNames.Length; i++ )
                {
                    //I think this may be superfluous because you can just check the options to see the connected evices
                }
            }

            return dbgString;
        }
    }
}
