using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace TestConnection
{
    public partial class MainWindow : Window
    {

        //paramètres du broker mqtt auquel on veut se connecter (et les identifiants si il en faut)
        string BrokerAddress = "172.31.254.100";
        int BrokerPort = 1883;
        string Username = "matheo";
        string Password = "matheo";
        string ClientID = Guid.NewGuid().ToString();
        IMqttClient clientmqtt;

        //variables dans lesquelles ont stocker les valeurs obtenues avec mqtt
        string humidity;
        string temperature;
        string decibels;

        async void ConnexionBroker()
        {
            //constructeur de la connexion
            var mqttnet = new MqttFactory();
            //creation du client
            clientmqtt = mqttnet.CreateMqttClient();

            //constructeur des paramètres de connexion
            var parametres_connexion_client = new MqttClientOptionsBuilder()
            .WithClientId(ClientID)
            .WithTcpServer(BrokerAddress, BrokerPort)
            .WithCredentials(Username, Password)
            .WithCleanSession();
            //variable qui stocke les paramètres de connexion
            var parametres_mqtt = parametres_connexion_client.Build();

            //liste de filtres mqtt
            var listeAbonnementsTopics = new List<MqttTopicFilter>();
            //liste ou on rentre les topics auxquels on veut s'abonner (attention mettre le chemin entier)
            List<string> topicsVoullus = new List<string>()
            {
            "Etage1/KM103/humidity",
            "Etage1/KM103/temperature",
            "Etage1/KM103/decibels"
            };
            //boucle qui crée un filtre pour chaque topic dans la liste topicsVoullus et qui l'ajoute a la liste des filtres
            foreach (var topic in topicsVoullus)
            {
                var filtre = new MqttTopicFilterBuilder().WithTopic(topic).Build();
                listeAbonnementsTopics.Add(filtre);
            }

            //tentative de connexion au broker mqtt avec les paramètres entrés puis d'abonnement aux topics selectionnés
            try
            {
                //tentative de connexion a un borker mqtt avec les paramètres entrés
                await clientmqtt.ConnectAsync(parametres_mqtt);
                MessageBox.Show("Connecté au broker");

                //constructeur des options de filtres (on donne en paramètre la liste des filtres)
                var optionsAbonnement = new MqttClientSubscribeOptions()
                {
                    TopicFilters = listeAbonnementsTopics
                };

                //fonction d'abonnement aux topics (avec en paramètres la liste des filtres créée)
                await clientmqtt.SubscribeAsync(optionsAbonnement);
                //fonction qui appelle la fonction GestionMessage a chaque fois qu'un message est reçu
                clientmqtt.ApplicationMessageReceivedAsync += GestionMessage;
                MessageBox.Show("Abonnée aux topics");
            }

            catch (Exception)
            {
                MessageBox.Show($"Echec de la configuration de la connexion");
            }
        }

        //fonction qui convertit le payload des messages attachés aux topics en chaine
        private Task GestionMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            switch (topic)
            {
                case "Etage1/KM103/humidity":
                    humidity = payload;
                    break;
                case "Etage1/KM103/temperature":
                    temperature = payload;
                    break;
                case "Etage1/KM103/decibels":
                    decibels = payload;
                    break;
            }
            MessageBox.Show($"Humidité : {humidity}\nTempérature : {temperature}\nDécibels : {decibels}");

            return Task.CompletedTask;
        }

        public MainWindow()
        {
            InitializeComponent();
            ConnexionBroker();
        }
    }
}