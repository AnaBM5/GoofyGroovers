using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Goofy_Server
{
    public partial class ServerUI : Form
    {
        private Server server;
        private bool isRunning;
        private Thread updateThread;

        public ServerUI()
        {
            InitializeComponent();
            isRunning = true;
            server = new Server();

            // Start the thread for updating data
            //while (isRunning)
            //    UpdateData();

            updateThread = new Thread(UpdateData);
            updateThread.Start();
        }

        private void UpdateData()
        {

        }

        private void UpdateUI()
        {
            // Clear previous data in the UI
            entityListView.Items.Clear();

            // Display current data of entities in the UI
            foreach (BlobEntity entity in Server.lobbyList.ElementAt(0).playerList)
            {
                ListViewItem item = new ListViewItem(new string[]
                {
                    entity.blobUserName,
                    entity.blobUserId.ToString(),
                    entity.blobUserColor.ToString(),
                    entity.isJumping.ToString(),
                    entity.velocity.ToString(),
                    entity.jumpStartPoint.ToString(),
                    entity.jumpEndPoint.ToString(),
                    entity.jumpTheta.ToString(),
                    entity.worldPosition.ToString(),
                });
                entityListView.Items.Add(item);
            }
        }

        private void deleteAll(object sender, EventArgs e)
        {
            // Clear all data
            Server.lobbyList.ElementAt(0).playerList.Clear();

            // Update UI to reflect changes
            UpdateUI();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the update thread when the form is closing
            isRunning = false;
            updateThread.Join(); // Wait for the thread to finish
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }
    }
}