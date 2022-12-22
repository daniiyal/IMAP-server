using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using IMAP_server;
using IMAP_server.DataBase.Entities;


namespace IMAPServerGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server Server { get; set; }

        private String IP { get; set; } = "127.0.0.1";

        private int Port { get; set; } = 143;

        public MainWindow()
        {
            InitializeComponent();
           // IP = Server.GetLocalIPAddress();
            IPinput.Text = IP;
            PortInput.Text = Port.ToString();

        }


        public async Task<List<ClientEntity>> GetClients()
        {
            List<ClientEntity> clients = await Server.Db.GetClientsAsync();
            return clients;
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            Server = new Server(IP, Port);
            Server.StartServer();
            await Server.ConnectClient();
        }

        private async void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            Server.StopServer();
            //await Server.ConnectClient();
        }

        private void SetAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            int port;

            if (!String.IsNullOrEmpty(IPinput.Text) && Int32.TryParse(PortInput.Text, out port))
            {
                IP = IPinput.Text;
                Port = port;
                return;
            }

            MessageBox.Show("Введите адрес и порт сервера");

        }

        private async void ShowUsersButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Server == null)
            {
                MessageBox.Show("Запустите сервер");
                return;
            }


            UsersPanel.Visibility = Visibility.Visible;
            ServerSettings.Visibility = Visibility.Hidden;

            

            var users = await GetClients();

            UsersNumber.Text = GetRightUsersNumString(users.Count);

            List<UserDTO> userList = new List<UserDTO>();

            foreach (var user in users)
            {
                userList.Add(new UserDTO(user.Name));                
            }

            usersDatagrid.ItemsSource = userList;


        }

        private string GetRightUsersNumString(int usersNum)
        {

            string users;
            switch (usersNum % 10)
            {
                case 1:
                    users = usersNum + " пользователь";
                    break;
                case 2:
                case 3:
                case 4:
                    users = usersNum + " пользователя";
                    break;
                default:
                    users = usersNum + " пользователей";
                    break;
            }

            return users;
        }

        private void ShowServerSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            UsersPanel.Visibility = Visibility.Hidden;
            ServerSettings.Visibility = Visibility.Visible;
        }

        private async void DeleteUserButton_OnClick(object sender, RoutedEventArgs e)
        {
            UserDTO obj = ((FrameworkElement)sender).DataContext as UserDTO;
            await Server.Db.DeleteUser(obj.Name);
        }
    }
}
