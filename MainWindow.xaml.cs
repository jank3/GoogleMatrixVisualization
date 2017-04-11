using Microsoft.Maps.MapControl.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApplication1.Diagram;
using WpfApplication1.Map;
using WpfApplication1.Providers;
using WpfApplication1.ViewModels;
using Dat = System.Data;            // System.Data.dll  
using SqC = System.Data.SqlClient;  // System.Data.dll  
using System.Configuration;
using System.Globalization;
using System;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private const string googleKey = "AIzaSyBRjf1o-vbwugl0MTKN64M6lDo9K_Mtr5c"; //qosit: AIzaSyBRjf1o-vbwugl0MTKN64M6lDo9K_Mtr5c //mia: AIzaSyCAXbsuXr2TOR4Q0fVUe4C4aDxnUW0F2wY //R: AJKnXv84fjrb0KIHawS0Tg

        public SqC.SqlConnection connection = new SqC.SqlConnection(ConfigurationSettings.AppSettings["SQLconnectionString"].ToString());

        //public partial class GetSelectedNodo
        //{
        public string SelectedNodo
        {
            get
            {
                return this.tbNodo.Text.ToString();

            }
        }
        // }


        /// <summary>
        /// Instantiates new <see cref="MainWindow"/> control.
        /// </summary>
        public MainWindow()
        {
            // Initialize data context
            // this.tbNodo.Text = "";
            this.DataContext = ViewModelProvider.MainViewModel;
            this.ViewModel.Nodes.CollectionChanged += Nodes_CollectionChanged;
            this.ViewModel.Connectors.CollectionChanged += Connectors_CollectionChanged;

            // Initialize UI
            this.InitializeComponent();

            // Loading DB distance matrix
            this.LoadDBConnectors();

            // Populate UI from data context
            //          this.AddConnectors(this.ViewModel.Connectors);
            //          this.AddNodes(this.ViewModel.Nodes);

            // Attach to events
            this.Map.PreviewMouseDown += Map_PreviewMouseDown;
        }

        private void LoadDBConnectors()
        {
            connection.Open();
            //this.WriteLine("SQL LoadDBConnectors : Connected successfully to mathip-chep-sql1.");
            System.Diagnostics.Debug.WriteLine("SQL LoadDBConnectors : Connected successfully to mathip-chep-sql1.");

            using (var command = new SqC.SqlCommand())
            {

                command.Connection = connection;
                command.CommandType = Dat.CommandType.Text;
                //command.CommandText = string.Format("select origen.ID_NODO, origen.Latitud, origen.Longitud, destino.ID_NODO, destino.Latitud, destino.Longitud, matriz.KM,matriz.TIEMPO from dbo.Q_TRIPSMATRIX matriz, dbo.Q_ACCOUNTNODE origen, dbo.Q_ACCOUNTNODE destino where matriz.origen=origen.ID_NODO and matriz.destino=destino.ID_NODO");
                command.CommandText = string.Format("select origen.ID_NODO, origen.Latitud, origen.Longitud, destino.ID_NODO, destino.Latitud, destino.Longitud, matriz.KM,matriz.TIEMPO from dbo.Q_TRIPSMATRIX matriz, dbo.Q_ACCOUNTNODE origen, dbo.Q_ACCOUNTNODE destino where matriz.origen = origen.ID_NODO and matriz.destino = destino.ID_NODO and origen.id_nodo = 'ES10_1000' and matriz.ID_CENTRO_SERVICIO = 'ES10_1000';");

                SqC.SqlDataReader reader = command.ExecuteReader();

                bool creaPrimerNodo = true;

                if (reader.HasRows)
                {
                    int connectionIndex = 0;
                    while (reader.Read())
                    {
                        //System.Diagnostics.Debug.WriteLine(reader.GetString(0) + "\t" + reader.GetString(1) + "\t" + reader.GetString(2) + "\t" + reader.GetString(3));

                        var node = new NodeObject()
                        {
                            X = float.Parse(reader.GetString(1), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US")),//location.Latitude
                            Y = float.Parse(reader.GetString(2), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US")), //location.Longitude
                            ID_NODO = string.Format("{0}", reader.GetString(0))
                        };

                        if (creaPrimerNodo)
                        {
                            this.ViewModel.Nodes.Add(node);
                            creaPrimerNodo = false;
                        }

                        var origin = node;
                        var originCollection = new List<NodeObject>() { origin };

                        var node2 = new NodeObject()
                        {
                            X = float.Parse(reader.GetString(4), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US")), //location.Latitude
                            Y = float.Parse(reader.GetString(5), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US")),  //location.Longitude
                            ID_NODO = string.Format("{0}", reader.GetString(3))
                        };
                        var destino = node2;
                        var destinoCollection = new List<NodeObject>() { destino };

                        this.ViewModel.Nodes.Add(node2);

                        this.ViewModel.Connectors.Add(new ConnectorObject
                        {
                            StartNode = origin,
                            //EndNode = this.ViewModel.Nodes[connectionIndex],
                            EndNode = destino,
                            Text = (float.Parse(reader.GetString(7), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US")) / 01.0).ToString("0.000000", CultureInfo.CreateSpecificCulture("en-US"))
                            //Text = string.Format("{0}", (weight.ElementAt(connectionIndex)))
                        });

                        connectionIndex++;
                        //if (connectionIndex > 800)
                        //    return;
                    }
                    this.ViewModel.connectorStart = this.ViewModel.Connectors.Count;

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Nothing to LOGS");
                }
            }
        }

        /// <summary>
        /// Handle the right click mouse down event on the map.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void Map_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // On right click, calculates the position based on 
            // mouse location on the map
            if (e.RightButton == MouseButtonState.Pressed)
                this.ViewModel.HandleClick(this.Map.ViewportPointToLocation(e.GetPosition(this.Map)));
        }

        /// <summary>
        /// Handles the connectors collection change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddConnectors(e.NewItems.OfType<ConnectorObject>());
        }

        /// <summary>
        /// Adds the specified connectors to the map.
        /// </summary>
        /// <param name="nodes">The connectors collection.</param>
        private void AddConnectors(IEnumerable<ConnectorObject> connectors)
        {
            foreach (var connector in connectors)
            {
                // Insert so that connectors render before nodes 
                this.Map.Children.Insert(0, connector.Line);

                // Creates the label for the connector one the map
                // Label will be located in the mid-point of the line
                var label = new TextBlock();
                label.Text = connector.Text;
                label.Foreground = Brushes.Black;
                label.FontSize = 16;
                MapLayer.SetPosition(label, MapFunctions.CalculateUsingHaversine(new Location(connector.TextX, connector.TextY), 0, 0));
                this.Map.Children.Add(label);
            }
        }

        /// <summary>
        /// Handles the nodes collection change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddNodes(e.NewItems.OfType<NodeObject>());
        }

        /// <summary>
        /// Adds the specified nodes to the map.
        /// </summary>
        /// <param name="nodes">The nodes collection.</param>
        private void AddNodes(IEnumerable<NodeObject> nodes)
        {
            foreach (var node in nodes)
                this.Map.Children.Add(node.Pin);
        }

        /// <summary>
        /// Gets the main view model.
        /// </summary>
        public MainViewModel ViewModel => this.DataContext as MainViewModel;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbNodo.Text))
                MessageBox.Show("Please entry a Valid ID_NODE!");
            else
            {
                if (this.ViewModel.connectorStart == this.ViewModel.Connectors.Count)
                    MessageBox.Show("There is not new Node to insert!");
                else
                {
                    MessageBox.Show(string.Format("Weights = {0}, Nodes = {1}, Connectors = {2}",this.ViewModel.weightTotal,this.ViewModel.Nodes.Count,this.ViewModel.Connectors.Count));
                   // InsertDBMatrixNewNode();
                }
            }
        }

        private void InsertDBMatrixNewNode()
        {
            try
            {
                var connection = new SqC.SqlConnection(ConfigurationSettings.AppSettings["SQLconnectionString"].ToString());
                connection.Open();

                using (var commandInsert = new SqC.SqlCommand())
                {
                    int InsertOK;
                    int seq = 0;

                    commandInsert.Connection = connection;
                    commandInsert.CommandType = Dat.CommandType.Text;

                    System.Diagnostics.Debug.WriteLine("Writing SQL MATRIX New Node");


                    foreach (var connector in  this.ViewModel.Connectors.Skip(this.ViewModel.connectorStart).Take(this.ViewModel.Connectors.Count - this.ViewModel.connectorStart))
                    {
                        commandInsert.CommandText = string.Format("INSERT INTO dbo.Q_TRIPSMATRIX (ID_CENTRO_SERVICIO, FECHA_HORA_ENVIO, ORIGEN, DESTINO, KM, TIEMPO) VALUES ('{0}','{1}','{2}','{3}','-','{4}')",
                        "XXX", seq.ToString(), connector.StartNode.ID_NODO, connector.EndNode.ID_NODO, connector.Text.Substring(0, 8));

                        InsertOK = commandInsert.ExecuteNonQuery();

                        if (InsertOK <= 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Error writing SQL MATRIX : {0},{1}", connector.StartNode.ToString(), connector.EndNode.ToString());
                        }
                        seq++;
                        if (seq > 658)
                            seq = seq;
                    }
                    commandInsert.Dispose();
                    // connection.Close();
                }
                //System.Diagnostics.Debug.WriteLine(this.Connectors.Count.ToString());
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error writing SQL MATRIX");
                Console.WriteLine(exc.ToString());
                Console.Error.WriteLine();
                System.Environment.Exit(-825);
                throw exc;
            }

        }
    }
}
