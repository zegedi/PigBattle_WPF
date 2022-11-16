using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigBattle.WPF.ViewModel
{
    public class InputCommandEventArgs : EventArgs
    {
        private String _command;

        public String Command { get { return _command; } }

        public InputCommandEventArgs(String command)
        {
            _command = command;
        }
    }
}
