﻿using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Lyrixator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            TopPanel.Opacity = 1;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            TopPanel.Opacity = 0;
            Lyrics.IsReadOnly = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            TrayIcon.Dispose();

            base.OnClosing(e);
        }

        private void OnTaskbarIconTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                MinimizeWindow();
            }
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                MaximizeWindow();
            }
        }

        private void MinimizeWindow()
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void MaximizeWindow()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = new PackIcon { Kind = PackIconKind.WindowMaximize };
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = new PackIcon { Kind = PackIconKind.WindowRestore };
            }
        }

        private void OnLyricsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Lyrics.IsReadOnly = false;
        }
    }
}
