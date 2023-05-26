using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Knd.Printer.CoreLibFrame45.Validete
{
    public class ValidationBehavior : Behavior<UIElement>
    {
        private int _errorCount;

        protected override void OnAttached()
        {
            if (base.AssociatedObject is FrameworkElement)
            {
                ((FrameworkElement)base.AssociatedObject).AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnValidationError));
            }
            base.OnAttached();
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                _errorCount++;
            }
            else if (e.Action == ValidationErrorEventAction.Removed)
            {
                _errorCount--;
            }
            GetViewModelBaseHandle().IsValid = _errorCount == 0;
        }

        private ViewModelBase GetViewModelBaseHandle()
        {
            if (((FrameworkElement)base.AssociatedObject).DataContext is ViewModelBase)
            {
                return ((FrameworkElement)base.AssociatedObject).DataContext as ViewModelBase;
            }
            return null;
        }
    }

}
