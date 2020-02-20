using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Thesis
{
    public class LabelViewModel: INotifyPropertyChanged
    {
        private string _labelText;

        public event PropertyChangedEventHandler PropertyChanged;

        public LabelViewModel()
        {
            _labelText = "";
        }

        public string LabelText
        {
            set
            {
                if (_labelText != value)
                {
                    _labelText = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LabelText"));
                }
            }
            get
            {
                return _labelText;
            }
        }
    }
}

