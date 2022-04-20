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

// **********************************

using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Costanti
        const int MILLISECONDS = 250;

        // Oggetti
        Socket          mSocket = null;
        DispatcherTimer mTimer  = null;

        public MainWindow()
        {
            // ************************************
            // Inizializzazione socket

            // SOCKET:
            // AddressFamily    : Enumeratore, specifica le modalità di lavoro. InterNetwork permette di lavorare con IPv4.
            // SocketType       : Enumeratore, Dgram (DataGram). Serve per creare una socket UPD.
            // ProtocolType     : Enumeratore, specifica il tipo di protocollo da utilizzare.
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // IP:
            IPAddress   local_address   = IPAddress.Any;                                    // Mittente
            IPEndPoint  local_endpoint  = new IPEndPoint(local_address.MapToIPv4(), 65000); // Creazione dell'indirizzo ip del mittente
            // I messaggi ora verranno ricevuti sulla porta 65000 (tra la 65000 e la 65500 le porte saranno sbloccate lato firewall)

            // Bind dell'indirizzo ip sulla socket
            mSocket.Bind(local_endpoint);

            // ************************************
            // Inizializzazione timer

            mTimer = new DispatcherTimer();

            mTimer.Tick     +=  new EventHandler(aggiorna_messaggi);    // Evento da eseguire quando passa il tempo
            mTimer.Interval =   new TimeSpan(0, 0, 0, 0, MILLISECONDS); // Ogni quanto eseguiure il metodo?
            mTimer.Start();                                             // Avvia il timer

            InitializeComponent();
        }

        private void aggiorna_messaggi(object sender, EventArgs e)
        {
            int nBytesAvailable = 0; // Numero di bytes ricevuti

            if ((nBytesAvailable = mSocket.Available) > 0) // Se ci sono bytes da processsare (nBytes diventa il numero di bytes)
            {
                int     nBytes = 0; //Numero di bytes effettivamente ricevuti
                string  sender_ip = "";
                string  message = "";
                byte[]  recieved_bytes = new byte[nBytesAvailable];   // Creo il buffer dove inseriremo i bytes ricevuti

                EndPoint sender_endpoint = new IPEndPoint(IPAddress.Any, 0);    // Non so chi sia chi ha inviato il messaggio

                nBytes      = mSocket.ReceiveFrom(recieved_bytes, ref sender_endpoint);     // Ricevo il messaggio, imposto sender_endpoint
                sender_ip   = ((IPEndPoint)(sender_endpoint)).Address.ToString();           // Qual'è l'ip del mittente?
                message     = Encoding.UTF8.GetString(recieved_bytes, 0, nBytes);           // Finalmente il messaggio

                // Scrivo il messaggio nella listbox
                lstMessaggi.Items.Add(
                        sender_ip + " -> " + message
                    );
            }
        }

        private void btnInvias_Click(object sender, RoutedEventArgs e)
        {
            IPAddress   remote_address  = IPAddress.Parse(txtIP.Text);                                             // Destinatario
            IPEndPoint  remote_endpoint = new IPEndPoint(remote_address.MapToIPv4(), int.Parse(txtPorta.Text));  // Endpoint del destinatario
            // I messaggi saranno inviati all'ip scelto sulla porta scelta

            // Conversione del messaggio in un array di byte
            byte[] byte_msg = Encoding.UTF8.GetBytes(txtMessaggio.Text);    // Uso la codifica UTF-8

            // Ora posso inviare il messaggio
            mSocket.SendTo(byte_msg, remote_endpoint);
        }
    }
}
