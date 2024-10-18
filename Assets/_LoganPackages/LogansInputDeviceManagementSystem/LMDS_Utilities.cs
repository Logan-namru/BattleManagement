using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogansMultiDeviceSystem
{
    public static class LMDS_Utilities
    {

    }

    [System.Serializable]
    public struct LMDS_Device
    {
        private string deviceName;
        public string DeviceName => deviceName;

        /// <summary>
        /// The index of this device as it appears in Input.GetJoystickNames()
        /// </summary>
        private int index_inDeviceList;

        /// <summary>
        /// joystick number as this device corresponds to the joystick keycode enumerations. IE: if 
        /// this is the first in the list, this value will be '1', corresponding to the 
        /// 'Joystick1Button...' enumerations
        /// </summary>
        private int joystickNumber;
        /// <summary>
        /// joystick number as this device corresponds to the joystick keycode enumerations. IE: if 
        /// this is the first in the list, this value will be '1', corresponding to the 
        /// 'Joystick1Button...' enumerations
        /// </summary>
        public int JoystickNumber => joystickNumber;
        /// <summary>
        /// The index of this device as it appears in Input.GetJoystickNames()
        /// </summary>
        public int Index_InDeviceList => index_inDeviceList;

        /// <summary>
        /// Saves index where this devices keycode enums start.
        /// </summary>
        private int index_keycodeEnumStart;

        private string h_axis_string;
        public string HorizontalAxisString => h_axis_string;

        private string v_axis_string;
        public string VerticalAxisString => v_axis_string;
        private static readonly LMDS_Device noneDevice = new LMDS_Device( -1, string.Empty );


        public LMDS_Device( int i, string n )
        {
            index_inDeviceList = i;
            deviceName = n;
            h_axis_string = "Horizontal";
            v_axis_string = "Vertical";

            if( i > -1 )
            {
                joystickNumber = i + 1;
                index_keycodeEnumStart = 330 + ( joystickNumber * 20 );
                h_axis_string += joystickNumber;
                v_axis_string += joystickNumber;

            }
            else
            {
                joystickNumber = -1;
                index_keycodeEnumStart = -1;
            }
        }

        /// <summary>
        /// Takes a 'catch all' joystick keycode (Keycodes from 330-350), and return the corresponding 
        /// keycode that would only target this device's number. Note: this is an expensive operation. 
        /// Don't use this for Update() functionality. Use it instead to cache keycodes.
        /// </summary>
        /// <param name="kc"></param>
        /// <returns>The specific keycode corresponding to this joystick's position. Keycode.None if the supplied keycode is outside the bounds of the 'JoystickButton' keycodes</returns>
        public KeyCode GetTargetedJoystickKeycode( KeyCode kc )
        {
            //Debug.Log( $"{deviceName}.GetTargetedJoystickKeycode('{kc}') start index: '{index_keycodeEnumStart}': '{(KeyCode)index_keycodeEnumStart}'" +
                //$"device list index: '{index_inDeviceList}'" );

            int kcIndex = (int)kc;
            //Debug.Log($"got kcIndex: '{kcIndex}', '{(KeyCode)kcIndex}'");
            if( kcIndex < 330 || kcIndex > 349 )
            {
                return KeyCode.None;
            }

            //Debug.Log($"returning: '{(index_keycodeEnumStart + (349 - kcIndex))}'");

            return (KeyCode)(index_keycodeEnumStart + (kcIndex - 330));
        }

        public static LMDS_Device none
        {
            get
            {
                return noneDevice;
            }
        }
    }
}