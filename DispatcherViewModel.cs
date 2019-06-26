using DisBotTelegram.BLL;
using DisBotTelegram.BLL.DTO;
using DisBotTelegram.BLL.Helper;
using DisBotTelegram.BLL.Interfaces;
using DisBotTelegram.BLL.Logic;
using DisBotTelegram.BLL.Services;
using DisBotTelegram.PL.Desktop.Helper;
using DisBotTelegram.PL.Desktop.Model;
using DisBotTelegram.PL.Desktop.ReleyCommand;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;

namespace DisBotTelegram.PL.Desktop.ViewModels
{
    public class DispatcherViewModel : BaseViewModel
    {
        #region Fields
        private ListBox _mainListBox;
        private LogicBot _botLogic;
        private UnityContainer _container;
        private ModelClientMessageService _modelClientMessageService;
        private ModelClientService _modelClientService;
        private ModelDespatcherMessageService _modelDespatcherMessageService;
        private ModelWorkTimeDispatcherService _modelWorkTimeDispatcherService;

        private bool _isConnect;
        private bool _isDisconnect;
        private bool _isSendMessage;

        private ObservableCollection<ClientInfo> _clients;

        private string _userName;
        private string _message;
        private ObservableCollection<ClientInfo> _clientsChat;

        private ClientInfo _сhoiceClient;

        #endregion

        #region Properties
        public ICommand SendMessageCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ObservableCollection<DisBotMessage> Messages { get; set; }
        public UserInfo UserInfo { get; set; }

        public ClientInfo ChoiceClient
        {
            get { return _сhoiceClient; }
            set
            {
                _сhoiceClient = value;
                OnPropertyChanged();
                if (!String.IsNullOrEmpty(ChoiceClient.Username))
                {
                    UserName = ChoiceClient.Username;
                }
                else if (!String.IsNullOrEmpty(ChoiceClient.LastName))
                {
                    UserName = ChoiceClient.LastName;
                }
                else if (!String.IsNullOrEmpty(ChoiceClient.FirstName))
                {
                    UserName = ChoiceClient.FirstName;
                }
                else
                    UserName = "Unknown name";
            }
        }
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }
        public string MessageChat
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        public bool IsConnect
        {
            get { return _isConnect; }
            set { _isConnect = value; OnPropertyChanged(); }
        }
        public bool IsDisconnect
        {
            get { return _isDisconnect; }
            set { _isDisconnect = value; OnPropertyChanged(); }
        }
        public bool IsSendMessage
        {
            get { return _isSendMessage; }
            set { _isSendMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ClientInfo> ClientsChat
        {
            get { return _clientsChat; }
            set { _clientsChat = value; OnPropertyChanged(); }
        }

        public ListBox MainListBox
        {
            get { return _mainListBox ?? (_mainListBox = new ListBox()); }
            set { _mainListBox = value; OnPropertyChanged(); }

        }
        #endregion

        #region Ctor
        public DispatcherViewModel()
        {
            _botLogic = new LogicBot();
            _container = new UnityContainer();
            _container.RegisterType<IClientMessageService, ClientMessageService>();
            _container.RegisterType<IClientService, ClientService>();
            _container.RegisterType<IDispatcherMessageService, DispatcherMessageService>();
            _container.RegisterType<IWorkTimeDispatcherService, WorkTimeDispatcherService>();
            _modelClientMessageService = new ModelClientMessageService(_container);
            _modelClientService = new ModelClientService(_container);
            _modelDespatcherMessageService = new ModelDespatcherMessageService(_container);
            _modelWorkTimeDispatcherService = new ModelWorkTimeDispatcherService(_container);

            _clients = _modelClientService.GetClients();
            _clientsChat = new ObservableCollection<ClientInfo>();

            _mainListBox = new ListBox();

            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                var window = Application.Current.Windows[i];
                if (window.Tag == null)
                {
                    continue;
                }
                else if (window.Tag.Equals("DispatcherWindow"))
                {
                    Application.Current.Windows[i].Closing += DispatcherViewModel_Closed;
                    break;
                }
            }

            Messages = new ObservableCollection<DisBotMessage>();
            _сhoiceClient = new ClientInfo();
            _isConnect = true;
            SendMessageCommand = new RelayCommand(OnSendMessageCommandExecute);
            ConnectCommand = new RelayCommand(OnConnectCommand);
            DisconnectCommand = new RelayCommand(OnDisconnectCommand);
            //EventPost();

            _botLogic.LogStart += _botLogic_LogStart;

            UserInfo = StaticLogicBot.UserInfo;
        }

        private void _botLogic_LogStart(object sender, BLL.Logic.EventsArgs.BotEventArgs e)
        {
           Application.Current.Dispatcher.Invoke( () =>
           {
               var countClientDB = 0;
               var coutClientList = 0;

               if (_clients.Count == 0)
               {
                   var addClient = new ClientInfo()
                   {
                       FirstName = _botLogic.Messages.FirstName,
                       LastName = _botLogic.Messages.LastName,
                       TelegramId = _botLogic.Messages.Id,
                       Username = _botLogic.Messages.UserName,
                   };

                   _modelClientService.Add(addClient);
                   _clients.Add(addClient);
                   _clientsChat.Add(addClient);
               }
               foreach (var client in _clients)
               {

                   if (client.TelegramId.Equals(_botLogic.Messages.Id))
                   {
                       break;
                   }
                   else if (!string.IsNullOrEmpty(client.TelegramId))
                   {
                       countClientDB++;
                   }

                   if (_clients.Count == countClientDB)
                   {
                       var addClient = new ClientInfo()
                       {
                           FirstName = e.Message.FirstName,
                           LastName = e.Message.LastName,
                           TelegramId = e.Message.Id,
                           Username = e.Message.UserName,
                       };
                       _modelClientService.Add(addClient);
                       _clients.Add(addClient);
                       _clientsChat.Add(addClient);
                       break;
                   }
               }

               var addClientList = new ClientInfo()
               {
                   FirstName = e.Message.FirstName,
                   LastName = e.Message.LastName,
                   TelegramId = e.Message.Id,
                   Username = e.Message.UserName,
               };


               if (_clientsChat.Count == 0)
               {
                   _clientsChat.Add(addClientList);
               }

               foreach (var item in _clientsChat)
               {
                   if (item.TelegramId.Equals(e.Message.Id))
                   {
                       break;
                   }
                   else if (!item.TelegramId.Equals(e.Message.Id))
                   {
                       coutClientList++;
                   }

                   if (coutClientList == _clientsChat.Count())
                   {
                       _clientsChat.Add(addClientList);
                       break;
                   }
               }
               var message = new DisBotMessage()
               {
                   Content = e.Message.Content,
                   Date = e.Message.Date,
                   LastName = e.Message.LastName,
                   FirstName = e.Message.FirstName,
                   UserName = e.Message.UserName,
                   Type = DisBotMessage.MessageType.OutMessage,
               };


               Messages.Add(message);
               _mainListBox.ScrollIntoView(message);

               _clients = _modelClientService.GetClients();
               var tmpId = 0;
               foreach (var item in _clients)
               {
                   if (item.TelegramId.Equals(e.Message.Id))
                   {
                       tmpId = item.Id;
                       break;
                   }
               }

               CheckUserName();

               var messageDB = new ClientMessageInfo()
               {
                   MessageClient = e.Message.Content,
                   TimeMassage = e.Message.Date,
                   UserId = UserInfo.Id,
                   ClientId = tmpId
               };
               _modelClientMessageService.Add(messageDB);
               if (!String.IsNullOrEmpty(UserName))
               {
                   IsSendMessage = true;
               }
           });
        }

        private void DispatcherViewModel_Closed(object sender, EventArgs e)
        {
            if (IsDisconnect)
            {
                _botLogic.StopReceiving();
            }
        }

        private void OnDisconnectCommand(object obj)
        {
            _botLogic.StopReceiving();
            IsConnect = true;
            IsDisconnect = false;
            IsSendMessage = false;
        }
        #endregion

        #region Commands
        private void OnConnectCommand(object obj)
        {
            _botLogic.ReciveMessage();
            IsConnect = false;
            IsDisconnect = true;
        }

        /*private void EventPost()
        {
            UserInfo = StaticLogicBot.UserInfo;
            Console.WriteLine();
            _botLogic.Log += buffer =>
            {
                var countClientDB = 0;
                var coutClientList = 0;

                if (_clients.Count == 0)
                {
                    var addClient = new ClientInfo()
                    {
                        FirstName = _botLogic.Messages.FirstName,
                        LastName = _botLogic.Messages.LastName,
                        TelegramId = _botLogic.Messages.Id,
                        Username = _botLogic.Messages.UserName,
                    };

                    _modelClientService.Add(addClient);
                    _clients.Add(addClient);
                    _clientsChat.Add(addClient);
                }//

                foreach (var client in _clients)
                {

                    if (client.TelegramId.Equals(_botLogic.Messages.Id))
                    {
                        break;
                    }
                    else if (!string.IsNullOrEmpty(client.TelegramId))
                    {
                        countClientDB++;
                    }

                    if (_clients.Count == countClientDB)
                    {
                        var addClient = new ClientInfo()
                        {
                            FirstName = _botLogic.Messages.FirstName,
                            LastName = _botLogic.Messages.LastName,
                            TelegramId = _botLogic.Messages.Id,
                            Username = _botLogic.Messages.UserName,
                        };
                        _modelClientService.Add(addClient);
                        _clients.Add(addClient);
                        _clientsChat.Add(addClient);
                        break;
                    }
                }

                var addClientList = new ClientInfo()
                {
                    FirstName = _botLogic.Messages.FirstName,
                    LastName = _botLogic.Messages.LastName,
                    TelegramId = _botLogic.Messages.Id,
                    Username = _botLogic.Messages.UserName,
                };


                if (_clientsChat.Count == 0)
                {
                    _clientsChat.Add(addClientList);
                }

                foreach (var item in _clientsChat)
                {
                    if (item.TelegramId.Equals(_botLogic.Messages.Id))
                    {
                        break;
                    }
                    else if (!item.TelegramId.Equals(_botLogic.Messages.Id))
                    {
                        coutClientList++;
                    }

                    if (coutClientList == _clientsChat.Count())
                    {
                        _clientsChat.Add(addClientList);
                        break;
                    }
                }
                var message = new DisBotMessage()
                {
                    Content = _botLogic.Messages.Content,
                    Date = _botLogic.Messages.Date,
                    LastName = _botLogic.Messages.LastName,
                    FirstName = _botLogic.Messages.FirstName,
                    UserName = _botLogic.Messages.UserName,
                    Type = DisBotMessage.MessageType.OutMessage,
                };

                Messages.Add(message);
                _mainListBox.ScrollIntoView(message);

                _clients = _modelClientService.GetClients();
                var tmpId = 0;
                foreach (var item in _clients)
                {
                    if (item.TelegramId.Equals(_botLogic.Messages.Id))
                    {
                        tmpId = item.Id;
                        break;
                    }
                }

                CheckUserName();

                var messageDB = new ClientMessageInfo()
                {
                    MessageClient = _botLogic.Messages.Content,
                    TimeMassage = _botLogic.Messages.Date,
                    UserId = UserInfo.Id,
                    ClientId = tmpId
                };
                _modelClientMessageService.Add(messageDB);
                if (!String.IsNullOrEmpty(UserName))
                {
                    IsSendMessage = true;
                }
            };
        }*/

        private void _mainListBox_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnSendMessageCommandExecute(object obj)
        {
            _mainListBox = obj as ListBox;
           

            if (String.IsNullOrEmpty(UserName))
            {
                return;
            }
            if (!String.IsNullOrEmpty(_сhoiceClient.TelegramId))
            {
                _botLogic.Bot_Send_Message(_сhoiceClient.TelegramId, _message);
                CheckUserName();
            }
            else
                _botLogic.Bot_Send_Message(_botLogic.Messages.Id, _message);
            var tmpId = 0;
            foreach (var item in _clients)
            {
                if (item.TelegramId.Equals(_botLogic.Messages.Id))
                {
                    tmpId = item.Id;
                    break;
                }
            }
            var message = new DisBotMessage()
            {
                Content = _message,
                Date = DateTime.Now,
                UserName = UserInfo.UserLogin,
                Type = DisBotMessage.MessageType.InMessage,
            };

            Messages.Add(message);
            _mainListBox.ScrollIntoView(message);

            var addDispatcherMessage = new DispatcherMessageInfo()
            {
                ClientId = tmpId,
                MessageDispather = _message,
                TimeMassage = DateTime.Now,
                UserId = UserInfo.Id
            };
            _modelDespatcherMessageService.Add(addDispatcherMessage);
            MessageChat = String.Empty;
        }

        private void CheckUserName()
        {
            if (!String.IsNullOrEmpty(_botLogic.Messages.UserName))
            {
                UserName = _botLogic.Messages.UserName;
            }
            else if (!String.IsNullOrEmpty(_botLogic.Messages.LastName))
            {
                UserName = _botLogic.Messages.LastName;
            }
            else if (!String.IsNullOrEmpty(_botLogic.Messages.FirstName))
            {
                UserName = _botLogic.Messages.FirstName;
            }
            else
                UserName = "Unknown name";
        }
        #endregion
    }
}
