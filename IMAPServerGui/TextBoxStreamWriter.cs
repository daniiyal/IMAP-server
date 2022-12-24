using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IMAPServerGui
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            Application.Current.Dispatcher.Invoke(() => {
                base.Write(value);
                _output.AppendText(value.ToString());
            });

        }
        public override Encoding Encoding { get; }
    }
}
