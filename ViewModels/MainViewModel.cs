using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApplication1.Diagram;
using WpfApplication1.Google;
using System.Collections.Generic;
using Microsoft.Maps.MapControl.WPF;

namespace WpfApplication1.ViewModels
{
    /// <summary>
    /// The view model for <see cref="MainWIndow"/>.
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private const string googleMatrixApiKey = "AIzaSyBRjf1o-vbwugl0MTKN64M6lDo9K_Mtr5c";  //qosIT: AIzaSyBRjf1o-vbwugl0MTKN64M6lDo9K_Mtr5c //mia:"AIzaSyCEThXqC4NsDs_Lpa9OB12YHztsoGV5pvs" //R: "AJKnXv84fjrb0KIHawS0Tg"
        private double scale = 1;

        /// <summary>
        /// Gets the collection of diagram nodes.
        /// </summary>
        public ObservableCollection<NodeObject> Nodes { get; } = new ObservableCollection<NodeObject>();

        /// <summary>
        /// Gets the collection of diagram connectors.
        /// </summary>
        public ObservableCollection<ConnectorObject> Connectors { get; } = new ObservableCollection<ConnectorObject>();

        /// <summary>
        /// Gets or sets the diagram scale.
        /// </summary>
        public double Scale
        {
            get
            {
                return this.scale;
            }
            set
            {
                this.scale = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Handles the mouse click on the map.
        /// This will add new node on given location and connect it to other nodes.
        /// </summary>
        /// <param name="location">The location that the click cooresponds to.</param>
        public void HandleClick(Location location)
        {
            var node = new NodeObject()
            {
                X = location.Latitude,
                Y = location.Longitude
            };

            this.Nodes.Add(node);
            this.ConnectPoint(node);
        }

        /// <summary>
        /// Connects the specified node with other nodes in the collection.
        /// </summary>
        /// <param name="node">The node.</param>
        private async void ConnectPoint(NodeObject node)
        {
            // Send the request with origin as specified node
            // and destinations all other nodes except specified origin node
            
            //***************************************************************************
            int TotalIEnum = this.Nodes.Count();
            int buclesorigins = 0;
            int resto25origins = TotalIEnum % 25;

            if (resto25origins != 0)
            {
                buclesorigins = TotalIEnum / 25;
            }
            else
            {
                buclesorigins = TotalIEnum / 25 + 1;
            }
            //***************************************************************************
            
            for (int i = 1; i <= buclesorigins; i++)
            {
                var origin = node;
                var originCollection = new List<NodeObject>() { origin };

                var weights = await new GoogleMatrixApiClient(googleMatrixApiKey).RequestMatrix(
                originCollection.Select(n => n.ToString()),
                this.Nodes.Skip(25 * (i - 1)).Take(25).Except(originCollection).Select(n => n.ToString()));

                //System.Threading.Thread.Sleep(200);
                // Ignore if nothing returned
                if (weights.Count() == 0) return;

                // There should only one row in response so we'll use that
                var weight = weights.First();
                for (var connectionIndex = 0; connectionIndex < weight.Count(); connectionIndex++)
                {
                    // Ignore 0 weight values
                    if (weight.ElementAt(connectionIndex) == 0) continue;

                    // Create connector between origin node and
                    // destination node where text is travel duration in minutes
                    this.Connectors.Add(new ConnectorObject
                    {
                        StartNode = origin,
                        EndNode = this.Nodes[connectionIndex+(25 * (i - 1))],
                        Text = (weight.ElementAt(connectionIndex) / 60.0).ToString("0.00") + "'"
                        //Text = string.Format("{0}", (weight.ElementAt(connectionIndex)))
                    });
                }
            }
            for (int i = 1; i <= buclesorigins; i++)
            {
                var origin = node;
                var originCollection = new List<NodeObject>() { origin };

                var weights = await new GoogleMatrixApiClient(googleMatrixApiKey).RequestMatrix( 
                                this.Nodes.Skip(25 * (i - 1)).Take(25).Except(originCollection).Select(n => n.ToString()),
                                originCollection.Select(n => n.ToString()));

                //System.Threading.Thread.Sleep(200);
                // Ignore if nothing returned
                if (weights.Count() == 0) return;

                // There should only one row in response so we'll use that
                var weight = weights.First();
                for (var connectionIndex = 0; connectionIndex < weight.Count(); connectionIndex++)
                {
                    // Ignore 0 weight values
                    if (weight.ElementAt(connectionIndex) == 0) continue;

                    // Create connector between origin node and
                    // destination node where text is travel duration in minutes
                    this.Connectors.Add(new ConnectorObject
                    {
                        StartNode = origin,
                        EndNode = this.Nodes[connectionIndex + (25 * (i - 1))],
                        Text = (weight.ElementAt(connectionIndex) / 60.0).ToString("0.00") + "'"
                        //Text = string.Format("{0}", (weight.ElementAt(connectionIndex)))
                    });
                }
            }
            System.Diagnostics.Debug.WriteLine(this.Connectors.Count.ToString());
        }
    }
}
