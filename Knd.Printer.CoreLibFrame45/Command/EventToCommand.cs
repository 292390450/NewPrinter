using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using Microsoft.Xaml.Behaviors;

namespace Knd.Printer.CoreLibFrame45.Command
{
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is EventToCommand eventToCommand2 && eventToCommand2.AssociatedObject != null)
            {
                eventToCommand2.EnableDisableElement();
            }
        }));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            OnCommandChanged(s as EventToCommand, e);
        }));

        public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register("MustToggleIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (s is EventToCommand eventToCommand && eventToCommand.AssociatedObject != null)
            {
                eventToCommand.EnableDisableElement();
            }
        }));

        private object _commandParameterValue;

        private bool? _mustToggleValue;

        public const string EventArgsConverterParameterPropertyName = "EventArgsConverterParameter";

        public static readonly DependencyProperty EventArgsConverterParameterProperty = DependencyProperty.Register("EventArgsConverterParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null));

        public const string AlwaysInvokeCommandPropertyName = "AlwaysInvokeCommand";

        public static readonly DependencyProperty AlwaysInvokeCommandProperty = DependencyProperty.Register("AlwaysInvokeCommand", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public object CommandParameter
        {
            get
            {
                return GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }

        public object CommandParameterValue
        {
            get
            {
                return _commandParameterValue ?? CommandParameter;
            }
            set
            {
                _commandParameterValue = value;
                EnableDisableElement();
            }
        }

        public bool MustToggleIsEnabled
        {
            get
            {
                return (bool)GetValue(MustToggleIsEnabledProperty);
            }
            set
            {
                SetValue(MustToggleIsEnabledProperty, value);
            }
        }

        public bool MustToggleIsEnabledValue
        {
            get
            {
                return (!_mustToggleValue.HasValue) ? MustToggleIsEnabled : _mustToggleValue.Value;
            }
            set
            {
                _mustToggleValue = value;
                EnableDisableElement();
            }
        }

        public bool PassEventArgsToCommand { get; set; }

        public IEventArgsConverter EventArgsConverter { get; set; }

        public object EventArgsConverterParameter
        {
            get
            {
                return GetValue(EventArgsConverterParameterProperty);
            }
            set
            {
                SetValue(EventArgsConverterParameterProperty, value);
            }
        }

        public bool AlwaysInvokeCommand
        {
            get
            {
                return (bool)GetValue(AlwaysInvokeCommandProperty);
            }
            set
            {
                SetValue(AlwaysInvokeCommandProperty, value);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            EnableDisableElement();
        }

        private FrameworkElement GetAssociatedObject()
        {
            return base.AssociatedObject as FrameworkElement;
        }

        private ICommand GetCommand()
        {
            return Command;
        }

        public void Invoke()
        {
            Invoke(null);
        }

        protected override void Invoke(object parameter)
        {
            if (!AssociatedElementIsDisabled() || AlwaysInvokeCommand)
            {
                ICommand command = GetCommand();
                object commandParameter = CommandParameterValue;
                if (commandParameter == null && PassEventArgsToCommand)
                {
                    commandParameter = ((EventArgsConverter == null) ? parameter : EventArgsConverter.Convert(parameter, EventArgsConverterParameter));
                }
                if (command != null && command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }

        private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
        {
            if (element != null)
            {
                if (e.OldValue != null)
                {
                    ((ICommand)e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
                }
                ICommand command = (ICommand)e.NewValue;
                if (command != null)
                {
                    command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
                }
                element.EnableDisableElement();
            }
        }

        private bool AssociatedElementIsDisabled()
        {
            FrameworkElement element = GetAssociatedObject();
            return base.AssociatedObject == null || (element != null && !element.IsEnabled);
        }

        private void EnableDisableElement()
        {
            FrameworkElement element = GetAssociatedObject();
            if (element != null)
            {
                ICommand command = GetCommand();
                if (MustToggleIsEnabledValue && command != null)
                {
                    element.IsEnabled = command.CanExecute(CommandParameterValue);
                }
            }
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            EnableDisableElement();
        }
    }

    public interface IEventArgsConverter
    {
        object Convert(object value, object parameter);
    }
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        private readonly Func<bool> _canExecute;

        private EventHandler _requerySuggestedLocal;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    EventHandler canExecuteChanged = _requerySuggestedLocal;
                    EventHandler handler2;
                    do
                    {
                        handler2 = canExecuteChanged;
                        EventHandler handler3 = (EventHandler)Delegate.Combine(handler2, value);
                        canExecuteChanged = Interlocked.CompareExchange(ref _requerySuggestedLocal, handler3, handler2);
                    }
                    while (canExecuteChanged != handler2);
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    EventHandler canExecuteChanged = _requerySuggestedLocal;
                    EventHandler handler2;
                    do
                    {
                        handler2 = canExecuteChanged;
                        EventHandler handler3 = (EventHandler)Delegate.Remove(handler2, value);
                        canExecuteChanged = Interlocked.CompareExchange(ref _requerySuggestedLocal, handler3, handler2);
                    }
                    while (canExecuteChanged != handler2);
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute.Invoke;
            if (canExecute != null)
            {
                _canExecute = canExecute.Invoke;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public virtual void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null)
            {
                _execute();
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public RelayCommand(Action<T> execute)
            : this(execute, (Func<T, bool>)null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute.Invoke;
            if (canExecute != null)
            {
                _canExecute = canExecute.Invoke;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            if (parameter == null && typeof(T).IsValueType)
            {
                return _canExecute(default(T));
            }
            if (parameter == null || parameter is T)
            {
                return _canExecute((T)parameter);
            }
            return false;
        }

        public virtual void Execute(object parameter)
        {
            object val = parameter;
            if (parameter != null && parameter.GetType() != typeof(T) && parameter is IConvertible)
            {
                val = Convert.ChangeType(parameter, typeof(T), null);
            }
            if (!CanExecute(val) || _execute == null)
            {
                return;
            }
            if (val == null)
            {
                if (typeof(T).IsValueType)
                {
                    _execute(default(T));
                }
                else
                {
                    _execute((T)val);
                }
            }
            else
            {
                _execute((T)val);
            }
        }
    }


}
