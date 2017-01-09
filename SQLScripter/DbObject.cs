using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLScripter
{
    public class DbObject : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value == _selected)
                    return;

                _selected = value;
                NotifyPropertyChanged("selected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
