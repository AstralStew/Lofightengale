using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{        
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewCommandList", menuName = "Paperticket/Create New Command List", order = 1)]
    public class CommandList : ScriptableObject {

        public string listName;

        public Command[] commandList;

    }

}