using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

using System.Windows.Media;
using System.Windows.Shapes;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Services;
using PanoramaApp1.Resources;
using Microsoft.Phone.Tasks;


namespace PanoramaApp1
{
    public class EventItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public virtual int user { get; set; }

        public int Location { get; set; }

    }
    public partial class MainPage : PhoneApplicationPage
    {
        // MobileServiceCollectionView implements ICollectionView (useful for databinding to lists) and 
        // is integrated with your Mobile Service to make it easy to bind your data to the ListView
        private MobileServiceCollection<EventItem, EventItem> items;

        private IMobileServiceTable<EventItem> eventTable = App.MobileService.GetTable<EventItem>();
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
        }

         private async void InsertEventItem(EventItem eventItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await eventTable.InsertAsync(eventItem);
            items.Add(eventItem);
        }

        private async void RefreshEventItems()
        {
            // This code refreshes the entries in the list view be querying the TodoItems table.
            // The query excludes completed TodoItems
            try
            {
                items = await eventTable.ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                MessageBox.Show(e.Message, "Error loading items", MessageBoxButton.OK);
            }

            ListItems.ItemsSource = items;
            this.ButtonSave.IsEnabled = true;
        }

        private async void UpdateCheckedEventItem(EventItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await eventTable.UpdateAsync(item);
            items.Remove(item);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshEventItems();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var eventItem = new EventItem { Name = NameInput.Text };
            InsertEventItem(eventItem);
        }

       
        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshEventItems();

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();    
            }
        }

        /// <summary>
        /// We must satisfy Maps API's Terms and Conditions by specifying
        /// the required Application ID and Authentication Token.
        /// See http://msdn.microsoft.com/en-US/library/windowsphone/develop/jj207033(v=vs.105).aspx#BKMK_appidandtoken
        /// </summary>
        private void MyMap_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
#warning Please obtain a valid application ID and authentication token.
#else
#error You must specify a valid application ID and authentication token.
#endif
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "__ApplicationID__";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "__AuthenticationToken__";

            //MyMap.CartographicMode = MapCartographicMode.Aerial;
            GetCurrentCoordinate();
        }




        #region mapCode


        /// <summary>
        /// Helper method to draw a single marker on top of the map.
        /// </summary>
        /// <param name="coordinate">GeoCoordinate of the marker</param>
        /// <param name="color">Color of the marker</param>
        /// <param name="mapLayer">Map layer to add the marker</param>
        private void DrawMapMarker(GeoCoordinate coordinate, Color color, MapLayer mapLayer)
        {
            // Create a map marker
            Polygon polygon = new Polygon();
            polygon.Points.Add(new Point(0, 0));
            polygon.Points.Add(new Point(0, 75));
            polygon.Points.Add(new Point(25, 0));
            polygon.Fill = new SolidColorBrush(color);

            // Enable marker to be tapped for location information
            polygon.Tag = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            //polygon.MouseLeftButtonUp += new MouseButtonEventHandler(Marker_Click);

            // Create a MapOverlay and add marker.
            MapOverlay overlay = new MapOverlay();
            overlay.Content = polygon;
            overlay.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay.PositionOrigin = new Point(0.0, 1.0);
            mapLayer.Add(overlay);
        }

        private async void GetCurrentCoordinate()
        {
            ShowProgressIndicator(AppResources.GettingLocationProgressText);
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;

            try
            {
                Geoposition currentPosition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
                _accuracy = currentPosition.Coordinate.Accuracy;

                Dispatcher.BeginInvoke(() =>
                {
                    MyCoordinate = new GeoCoordinate(currentPosition.Coordinate.Latitude, currentPosition.Coordinate.Longitude);
                    DrawMapMarkers();
                    MyMap.SetView(MyCoordinate, 10, MapAnimationKind.Parabolic);
                });
            }
            catch (Exception)
            {
                // Couldn't get current location - location might be disabled in settings
                MessageBox.Show(AppResources.LocationDisabledMessageBoxText, AppResources.ApplicationTitle, MessageBoxButton.OK);
            }
            HideProgressIndicator();
        }

        /// <summary>
        /// Method to draw markers on top of the map. Old markers are removed.
        /// </summary>
        private void DrawMapMarkers()
        {
            MyMap.Layers.Clear();
            MapLayer mapLayer = new MapLayer();

            // Draw marker for current position
            if (MyCoordinate != null)
            {
                DrawAccuracyRadius(mapLayer);
                DrawMapMarker(MyCoordinate, Colors.Red, mapLayer);
            }

            // Draw markers for location(s) / destination(s)
            for (int i = 0; i < MyCoordinates.Count; i++)
            {
                DrawMapMarker(MyCoordinates[i], Colors.Blue, mapLayer);
            }

            // Draw markers for possible waypoints when directions are shown.
            // Start and end points are already drawn with different colors.
            if (_isDirectionsShown && MyRoute.LengthInMeters > 0)
            {
                for (int i = 1; i < MyRoute.Legs[0].Maneuvers.Count - 1; i++)
                {
                    DrawMapMarker(MyRoute.Legs[0].Maneuvers[i].StartGeoCoordinate, Colors.Purple, mapLayer);
                }
            }

            MyMap.Layers.Add(mapLayer);
        }

        /// <summary>
        /// Helper method to draw location accuracy on top of the map.
        /// </summary>
        /// <param name="mapLayer">Map layer to add the accuracy circle</param>
        private void DrawAccuracyRadius(MapLayer mapLayer)
        {
            // The ground resolution (in meters per pixel) varies depending on the level of detail 
            // and the latitude at which it’s measured. It can be calculated as follows:
            double metersPerPixels = (Math.Cos(MyCoordinate.Latitude * Math.PI / 180) * 2 * Math.PI * 6378137) / (256 * Math.Pow(2, MyMap.ZoomLevel));
            double radius = _accuracy / metersPerPixels;

            Ellipse ellipse = new Ellipse();
            ellipse.Width = radius * 2;
            ellipse.Height = radius * 2;
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(75, 200, 0, 0));

            MapOverlay overlay = new MapOverlay();
            overlay.Content = ellipse;
            overlay.GeoCoordinate = new GeoCoordinate(MyCoordinate.Latitude, MyCoordinate.Longitude);
            overlay.PositionOrigin = new Point(0.5, 0.5);
            mapLayer.Add(overlay);
        }

        /// <summary>
        /// Helper method to show progress indicator in system tray
        /// </summary>
        /// <param name="msg">Text shown in progress indicator</param>
        private void ShowProgressIndicator(String msg)
        {
            if (ProgressIndicator == null)
            {
                ProgressIndicator = new ProgressIndicator();
                ProgressIndicator.IsIndeterminate = true;
            }
            ProgressIndicator.Text = msg;
            ProgressIndicator.IsVisible = true;
            SystemTray.SetProgressIndicator(this, ProgressIndicator);
        }

        /// <summary>
        /// Helper method to hide progress indicator in system tray
        /// </summary>
        private void HideProgressIndicator()
        {
            ProgressIndicator.IsVisible = false;
            SystemTray.SetProgressIndicator(this, ProgressIndicator);
        }

        // Progress indicator shown in system tray
        private ProgressIndicator ProgressIndicator = null;

        /// <summary>
        /// Accuracy of my current location in meters;
        /// </summary>
        private double _accuracy = 0.0;

        // My current location
        private GeoCoordinate MyCoordinate = null;

        // List of coordinates representing search hits / destination of route
        private List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();

        /// <summary>
        /// True when directions are shown, otherwise false
        /// </summary>
        private bool _isDirectionsShown = false;

        // Route information
        private Route MyRoute = null;

        #endregion

    }
}