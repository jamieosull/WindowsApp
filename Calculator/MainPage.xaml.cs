﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Calculator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Geolocator _myGeo;

   
        public MainPage()
        {
            this.InitializeComponent();
            setupGeoLecationNow();

            result.Text = 0.ToString();
        }

        // location
        private async void setupGeoLecationNow()
        {
            // ask permission to access GPS data
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Unspecified:
                    tblStatus.Text = "Unspecified Error";
                    break;
                case GeolocationAccessStatus.Allowed:
                    tblStatus.Text = "Initialising Location Data";
                    // now set up the events,
                    _myGeo = new Geolocator();
                    // set up the intervals
                    _myGeo.DesiredAccuracy = PositionAccuracy.High;
                    _myGeo.DesiredAccuracyInMeters = 1;

                    // time in milliseconds - report
                    _myGeo.ReportInterval = 600000;


                    // events - run on the GPS thread, not the UI
                    // so update the ui using lambda events
                    _myGeo.StatusChanged += _myGeo_StatusChanged;

                    _myGeo.PositionChanged += _myGeo_PositionChanged;

                    // get the location now
                    // return GeoCoordinate

                    Geoposition pos = await _myGeo.GetGeopositionAsync();
                    updateUILocation(pos);


                    break;
                case GeolocationAccessStatus.Denied:
                    tblStatus.Text = "Access Denied :-(";
                    break;
                default:
                    break;
            }
        }

        private void updateUILocation(Geoposition pos)
        {
            TextBlock tblPosition;
            tblPosition = new TextBlock();
            tblPosition.TextWrapping = TextWrapping.Wrap;
            tblPosition.Name = "tblPosition";
            tblPosition.Text = "Latitude: " +
                pos.Coordinate.Point.Position.Latitude.ToString() +
                Environment.NewLine
                + "Longitude: " +
                pos.Coordinate.Point.Position.Longitude.ToString();

            spCoordinates.Children.Add(tblPosition);



        }

        private void _myGeo_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            // do some interesting stuff here.

        }

        private async void _myGeo_StatusChanged(Geolocator sender,
            StatusChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    updateUIStatus(args);
                });
        }

        private void updateUIStatus(StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case PositionStatus.Ready:
                    tblStatus.Text = "Ready";
                    break;
                case PositionStatus.Initializing:
                    tblStatus.Text = "Initialising";
                    break;
                case PositionStatus.Disabled:
                    tblStatus.Text = "GPS Disabled - please fix";
                    break;
                case PositionStatus.NoData:
                case PositionStatus.NotInitialized:
                case PositionStatus.NotAvailable:
                default:
                    tblStatus.Text = "GPS Data not available";
                    break;
            }
        }



        // calculator
        private void AddNumberToResult(double number)
        {
            if (char.IsNumber(result.Text.Last()))
            {
                if (result.Text.Length == 1 && result.Text == "0")
                {
                    result.Text = string.Empty;
                }
                result.Text += number;

            }
            else
            {
                if (number != 0)
                {
                    result.Text += number;
                }
            }
        }


        enum Operation { MINUS= 1, PLUS =2, DIV =3, TIMES =4, NUMBER =5 }
        private void AddOperationToResult(Operation operation)
        {
            if (result.Text.Length == 1 && result.Text == "0") return;

            if (!char.IsNumber(result.Text.Last()))
            {
                result.Text = result.Text.Substring(0, result.Text.Length -1);
            }

            switch (operation)
            {
                case Operation.MINUS: result.Text += "-"; break;
                case Operation.PLUS: result.Text += "+"; break;
                case Operation.DIV: result.Text += "/"; break;
                case Operation.TIMES: result.Text += "*"; break;
            }
        }

        private void btn7_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(7);

        }

        private void btn8_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(8);

        }

        private void btn9_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(9);

        }

        private void btnDiv_Click(object sender, RoutedEventArgs e)
        {
            AddOperationToResult(Operation.DIV);

        }
        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(4);

        }
        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(5);

        }
        private void btn6_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(6);

        }
        private void btnTimes_Click(object sender, RoutedEventArgs e)
        {
            AddOperationToResult(Operation.TIMES);

        }
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(1);

        }
        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(2);

        }
        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(3);

        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            AddOperationToResult(Operation.MINUS);

        }

        #region Equals
        private class Operand
        {
            public Operation operation = Operation.NUMBER; //default
            public double value = 0;

            public Operand left = null;
            public Operand right = null;
        }

        // Get expression from result.Text and build a tree with it!
        private Operand BuildTreeOperand()
        {
            Operand tree = null;

            string expression = result.Text;
            if (!char.IsNumber(expression.Last()))
            {
                expression = expression.Substring(0, expression.Length - 1);

            }

            string numberStr = string.Empty;
            foreach (char c in expression.ToCharArray())
            {
                if (char.IsNumber(c) || c == '-' || numberStr == string.Empty && c == '-')
                {
                    numberStr += c;
                }//if
                else
                {
                    AddOperationToTree(ref tree, new Operand() { value = double.Parse(numberStr) });
                    numberStr = string.Empty;

                    Operation op = Operation.MINUS; // default
                    switch (c)
                    {
                        case '-': op = Operation.MINUS; break;
                        case '+': op = Operation.PLUS; break;
                        case '/': op = Operation.DIV; break;
                        case '*': op = Operation.TIMES; break;

                    }
                    AddOperationToTree(ref tree, new Operand() { operation = op });

                }//else
            }//end of foreach

            //last number
            AddOperationToTree(ref tree, new Operand() { value = double.Parse(numberStr) });
            return tree;
        }//end of BuildTreeOperand


        private void AddOperationToTree(ref Operand tree, Operand elem)
        {
            if (tree == null)
            {
                tree = elem;
            }
            else
            {
                if (elem.operation < tree.operation)
                {
                    Operand auxTree = tree;
                    tree = elem;
                    elem.left = auxTree;
                }
                else
                {
                    AddOperationToTree(ref tree.right, elem); // recursive
                }

            }

        }


        private double Calc(Operand tree)
        {
            if (tree.left == null && tree.right == null)// its a number!
            {
                return tree.value;

            }
            else // its an operation (-,+,/,*)
            {
                double subResult = 0;
                switch (tree.operation)
                {
                    case Operation.MINUS: subResult = Calc(tree.left) - Calc(tree.right); break;// recursive
                    case Operation.PLUS: subResult = Calc(tree.left) + Calc(tree.right); break;
                    case Operation.DIV: subResult = Calc(tree.left) / Calc(tree.right); break;
                    case Operation.TIMES: subResult = Calc(tree.left) * Calc(tree.right); break;

                }
                return subResult;
            }
        }

        private void btnEquals_Click(object sender, RoutedEventArgs e)
        {

            // gate
            if (string.IsNullOrEmpty(result.Text)) return;

            Operand tree = BuildTreeOperand(); // from string in result.text

            double value = Calc(tree); // evaluate tree to calculate final result

            result.Text = value.ToString();


        }
        #endregion Equal
        private void btn0_Click(object sender, RoutedEventArgs e)
        {
            AddNumberToResult(0);

        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            result.Text = 0.ToString();

        }
        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            AddOperationToResult(Operation.PLUS);

        }
        private void Button_Click_Conv(object sender, RoutedEventArgs e)
        {
            double ans = Convert.ToDouble(TextBox_Amount.Text) * Convert.ToDouble(TextBox_ExchangeRate.Text);
            TextBox_Ans.Text = ans.ToString();
        }
       

    }



}
