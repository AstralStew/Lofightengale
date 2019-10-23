using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    [System.Serializable]
    public class Command
    {

        public string commandName;

        public bool commandEnabled;

        public string[] commandInputs;
        //[HideInInspector]
        public NativeInput[] commandSteps;      // Create at compile (start of runtime atm)

        public Sprite commandImage;

        [Tooltip("NOTE: this is in frames, not seconds")]
        public int recoveryLength;

        public bool debug;

    }

}
