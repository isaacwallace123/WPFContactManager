﻿using System;
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
using System.Windows.Shapes;

namespace WPFContactManager
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        public ViewWindow(string FirstName, string LastName, string Phone, string Email)
        {
            InitializeComponent();

            FirstText.Text = FirstName;
            LastText.Text = LastName;
            PhoneText.Text = Phone;
            EmailText.Text = Email;
        }
    }
}
