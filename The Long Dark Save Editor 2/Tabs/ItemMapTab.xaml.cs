using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using The_Long_Dark_Save_Editor_2.Game_data;
using The_Long_Dark_Save_Editor_2.Helpers;

namespace The_Long_Dark_Save_Editor_2.Tabs
{

    public partial class ItemMapTab : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="SelectedObject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            nameof(SelectedObject),
            typeof(string),
            typeof(ItemMapTab),
                                   new FrameworkPropertyMetadata( // Property metadata
                                default(string), // default value
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | // Flags
                                    FrameworkPropertyMetadataOptions.Journal,
                                new PropertyChangedCallback(OnSelectedObjectPropertyChanged),    // property changed callback
                                new CoerceValueCallback(CoerceValue),
                                true, // IsAnimationProhibited
                                UpdateSourceTrigger.PropertyChanged   // DefaultUpdateSourceTrigger
                                ));

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string SelectedObject
        {
            get
            {
                return (string)this.GetValue(SelectedObjectProperty);
            }

            set
            {
                this.SetValue(SelectedObjectProperty, value);
            }
        }


        /// <summary>
        /// Called when the <see cref="SelectedObject" /> has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private static void OnSelectedObjectPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ItemMapTab)sender).UpdateMap(true);
            ((ItemMapTab)sender).InvalidateVisual();
        }


        static object CoerceValue(DependencyObject sender, object e)
        {
            return e;
        }


        private MapInfo mapInfo;
        private bool mouseDown;
        private Point clickPosition;
        private Point lastMousePosition;

        private Point playerPosition;
        private Point itemPosition;

        public ItemMapTab()
        {
            InitializeComponent();

            MainWindow.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainWindow.Instance.CurrentSave))
                {
                    Debug.WriteLine("Currentsave changed");
                    if (MainWindow.Instance.CurrentSave == null)
                    {
                        UpdateMap(true);
                        return;
                    }

                    UpdateMap();
                    var saveGamePosition = MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition;
                    saveGamePosition.CollectionChanged += (sender2, e2) =>
                    {

                        if ((e2.NewStartingIndex == 0 && saveGamePosition[0] != (float)playerPosition.X) || (e2.NewStartingIndex == 2 && saveGamePosition[2] != (float)playerPosition.Y))
                        {
                            //playerPosition.X = saveGamePosition[0];
                            //playerPosition.Y = saveGamePosition[2];
                            //UpdatePlayerPosition();
                        }
                    };
                }
            };

        }

        private void UpdateMap(bool reset = false)
        {
            playerPosition = new Point(MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0], MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2]);

            if (MainWindow.Instance.CurrentSave == null)
                return;

            if (!IsLoaded)
                return;

            if (SelectedObject != null)// && Enum.TryParse<RegionsWithMap>(SelectedObject, out var region))
            {
                listBox1.ItemsSource = null;

                if (MainWindow.Instance.CurrentSave.MainRegions.TryGetValue(SelectedObject, out var item))
                {
                    var items = item.GearManagerData?.Items?.ToList();
                    items.RemoveAll(p => p.m_PrefabName == "GEAR_CattailTinder");
                    listBox1.ItemsSource = items.OrderBy(p => p.InGameName);
                }
            }

            if (SelectedObject == null)
            {
                mapImage.Source = null;
                mapInfo = null;
                player.Visibility = Visibility.Hidden;
                
                return;
            }
            //if (!MapDictionary.MapExists(SelectedObject))
            //{
            //    mapImage.Source = null;
            //    mapInfo = null;
            //    player.Visibility = Visibility.Hidden;

            //    return;
            //}

            if (reset)
            {
                player.Visibility = Visibility.Visible;

                var imgSource = ((Image)Resources[SelectedObject])?.Source;

                if (imgSource == null)
                    imgSource = ((Image)Resources["blank"]).Source;

                if (MapDictionary.GetMapInfo(SelectedObject) is MapInfo mi)
                    mapInfo = mi;
                else
                    mapInfo = MapDictionary.GetMapInfo("blank");

                mapImage.Source = imgSource;
                mapImage.Width = mapInfo.width;
                mapImage.Height = mapInfo.height;

                double wScale = canvas.ActualWidth / mapInfo.width;
                double hScale = canvas.ActualHeight / mapInfo.height;
                scaleMap.ScaleX = Math.Max(Math.Min(wScale, hScale), 0.5);
                scaleMap.ScaleY = Math.Max(Math.Min(wScale, hScale), 0.5);

                scaleOfPlayerIcon.ScaleX = 1 / scaleMap.ScaleX;
                scaleOfPlayerIcon.ScaleY = 1 / scaleMap.ScaleY;

                scaleOfItemIcon.ScaleX = 1 / scaleMap.ScaleX;
                scaleOfItemIcon.ScaleY = 1 / scaleMap.ScaleY;
            }

            UpdatePlayerPosition();
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mapInfo == null) return;

            mouseDown = true;
            clickPosition = e.GetPosition(canvas);
            lastMousePosition = clickPosition;
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mapInfo == null) return;

            mouseDown = false;
            canvas.ReleaseMouseCapture();
            if (e.GetPosition(canvas) == clickPosition)
            {
                //playerPosition = mapInfo.ToRegion(e.GetPosition(mapImage));
                //UpdatePlayerPosition();
                //MainWindow.Instance.CurrentSave.Boot.m_SceneName.Value = SelectedObject;
                //MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0] = (float)playerPosition.X;
                //MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2] = (float)playerPosition.Y;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mapInfo == null) return;

            if (mouseDown)
            {
                canvas.CaptureMouse();
                var mousePos = e.GetPosition(canvas);

                translateMap.X += (mousePos.X - lastMousePosition.X);
                translateMap.Y += (mousePos.Y - lastMousePosition.Y);
                lastMousePosition = mousePos;
            }
        }

        private void canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (mapInfo == null) return;

            double zoom = e.Delta > 0 ? .3 * scaleMap.ScaleX : -.3 * scaleMap.ScaleX;
            
            var x = e.GetPosition(mapLayer).X / mapLayer.ActualWidth;
            var y = e.GetPosition(mapLayer).Y / mapLayer.ActualHeight;
            x = Math.Max(Math.Min(x, 1), 0);
            y = Math.Max(Math.Min(y, 1), 0);
            var dX = (x - mapLayer.RenderTransformOrigin.X) * mapLayer.ActualWidth * (1 - scaleMap.ScaleX);
            var dY = (y - mapLayer.RenderTransformOrigin.Y) * mapLayer.ActualHeight * (1 - scaleMap.ScaleY);

            translateMap.X -= dX;
            translateMap.Y -= dY;
            mapLayer.RenderTransformOrigin = new Point(x, y);
            
            scaleMap.ScaleX += zoom;
            scaleMap.ScaleY += zoom;

            scaleOfPlayerIcon.ScaleX = 1 / scaleMap.ScaleX;
            scaleOfPlayerIcon.ScaleY = 1 / scaleMap.ScaleY;

            scaleOfItemIcon.ScaleX = 1 / scaleMap.ScaleX;
            scaleOfItemIcon.ScaleY = 1 / scaleMap.ScaleY;
        }

        private void UpdatePlayerPosition()
        {
            var pPoint = mapInfo.ToLayer(playerPosition);

            Canvas.SetLeft(player, pPoint.X);
            Canvas.SetTop(player, pPoint.Y);

            var iPoint = mapInfo.ToLayer(itemPosition);

            Canvas.SetLeft(cross, iPoint.X);
            Canvas.SetTop(cross, iPoint.Y);

        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap(true);
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(listBox1.SelectedItem is InventoryItemSaveData inventory)
            {
                itemPosition.X = inventory.GearNew.m_Position[0];
                itemPosition.Y = inventory.GearNew.m_Position[2];

                UpdateMap(false);
            }
        }
    }
}
