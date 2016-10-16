using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Heathmill.WpfUtilities
{
    /// <summary>
    /// Josh Smith's RelayCommand from MSDN magazine
    /// http://msdn.microsoft.com/en-us/magazine/dd419663.aspx
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action execute)
            : this(na => execute())
        {
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(na => execute(), na => canExecute())
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            try
            {
                OnExecute(parameter);
            }
            catch (Exception ex)
            {
                OnExecuteException(ex);
            }
        }

        protected virtual void OnExecuteException(Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Unexpected Exception");
        }

        protected virtual void OnExecute(object parameter)
        {
            UpdateFocusedField();
            _execute(parameter);
        }

        #endregion // ICommand Members

        //Karl's UpdateFocusedField Toolbar Fix
        //Shouldn't hurt to have it here
        public static void UpdateFocusedField()
        {
            TextBox f = Keyboard.FocusedElement as TextBox;
            if (f != null)
            {
                BindingExpression expr = f.GetBindingExpression(TextBox.TextProperty);
                if (expr != null) expr.UpdateSource();
            }
        }
    }
}
