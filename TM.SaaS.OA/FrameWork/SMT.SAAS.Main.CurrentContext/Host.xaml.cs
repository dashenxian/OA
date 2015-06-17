﻿using System;
using System.Windows.Controls;
using System.Windows.Browser;
using System.Windows;
// 内容摘要: 提供界面实现的宿主,主外壳。承载MainPage
namespace SMT.SAAS.Main.CurrentContext
{
    /// <summary>
    /// 系统容器外壳，用于承载MainPage、Login等
    /// 在加载顺序上为RootVisual->Host->Login 成功后->MainPage
    /// </summary>
    public partial class Host : UserControl
    {
        public Host()
        {
            InitializeComponent();
            systemMessageArea.Visibility = Visibility.Collapsed;
            //初始化系统继承引导程序
            //ViewModel.Context.Host = this;
            //if (ViewModel.Context.Managed == null)
            //    ViewModel.Context.Managed = new ViewModel.Managed();
        }
        
        public FrameworkElement SilverlightHostRoot
        {
            get { return Root; }
            //set { silverlightHostRoot = value; }
        }

        public void setroot(UserControl userControl)
        {
            //默认设置登录页面。
            SetRoot(userControl);


              
        }

        /// <summary>
        /// 设置外壳容器的内容。
        /// </summary>
        /// <param name="userControl">当前要设置的容器。</param>
        public void SetRoot(UserControl userControl)
        {
            Root.Children.Clear();
            Root.Children.Add(userControl);
        }

        /// <summary>
        /// 设置外壳容器的内容。
        /// </summary>
        /// <param name="Content">当前要设置的容器。</param>
        public void SetRootVisual(System.Windows.UIElement Content)
        {
            UserControl uc = Content as UserControl;
            if (uc == null)
                throw new Exception("Content is not a UserControl");

            SetRoot(uc);
        }

        /// <summary>
        /// 系统注销方法，注销后将返回到登录界面。
        /// 注销动作由MainPage触发，将情况用户的所有相关数据。
        /// </summary>
        public void LoginOff()
        {
            AppContext.LogOff = false;

            string strHost = SMT.SAAS.Main.CurrentContext.Common.HostIP;
            string strUrl = "http://" + strHost + "/";

            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri(strUrl);
            wd.Navigate(uri);
            //Common.AppContext.LogOff = false;
            //ViewModel.Context.Host.SetRootVisual(new LoginPart.Login());
        }

        private void Button_HideLog(object sender, System.Windows.RoutedEventArgs e)
        {
            systemMessageArea.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Button_ShowLog(object sender, System.Windows.RoutedEventArgs e)
        {
            double x = (double)HtmlPage.Window.Eval("screen.width");//得到屏幕工作区域宽度
            double y = (double)HtmlPage.Window.Eval("screen.height");//得到屏幕工作区域宽度
            txtSystemMessage.Height = y;
            txtSystemMessage.Width = x;
            txtSystemMessage.Visibility = System.Windows.Visibility.Visible;

            systemMessageArea.Height = y;
            systemMessageArea.Width = x;
            systemMessageArea.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// 显示系统提示信息输出框
        /// </summary>
        [ScriptableMember]
        public void OpenHostMessage()
        {
            txtSystemMessage.Height = 100;
            txtSystemMessage.Visibility = System.Windows.Visibility.Visible;
            systemMessageArea.Height = 100;
            systemMessageArea.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            txtSystemMessage.Text = string.Empty;
        }
    }
}
