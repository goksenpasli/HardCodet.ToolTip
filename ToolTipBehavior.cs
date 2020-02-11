using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Hardcodet.ToolTips
{

	[ContentProperty("Content")]
	[DefaultProperty("Content")]
	public class ToolTipBehavior : Behavior<FrameworkElement>
	{
		private readonly DispatcherTimer timer;
		private Action timerAction;
		private Popup popup;
		private HeaderedToolTip toolTip;

		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(string.Empty));

		[Description("The main tooltip content.")]
		[Category("Common")]
		[DisplayName("Content")]
		[Bindable(true)]
		public object Content
		{
			get
			{
				return GetValue(ContentProperty);
			}
			set
			{
				SetValue(ContentProperty, value);
			}
		}

		public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(ToolTipBehavior), new FrameworkPropertyMetadata());
		[Description("Bind another property.")]
		[Category("Common")]
		[DisplayName("Tag")]
		[Bindable(true)]
		public object Tag
		{
			get
			{
				return GetValue(TagProperty);
			}
			set
			{
				SetValue(TagProperty, value);
			}
		}

		public static readonly DependencyProperty ContentStringFormatProperty = DependencyProperty.Register(" ContentStringFormat", typeof(string), typeof(ToolTipBehavior), new FrameworkPropertyMetadata((string)null));

		[Description("Formatting of a content string.")]
		[Category("Common")]
		[DisplayName("Content StringFormat")]
		[Bindable(true)]
		public string ContentStringFormat
		{
			get
			{
				return (string)GetValue(ContentStringFormatProperty);
			}
			set
			{
				SetValue(ContentStringFormatProperty, value);
			}
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Header", typeof(object), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(string.Empty));

		[Description("Optional tooltip header")]
		[Category("Common")]
		[DisplayName("Header")]
		[Bindable(true)]
		public object Header
		{
			get
			{
				return GetValue(TitleProperty);
			}
			set
			{
				SetValue(TitleProperty, value);
			}
		}

		public static readonly DependencyProperty TitleStringFormatProperty = DependencyProperty.Register("TitleStringFormat", typeof(string), typeof(ToolTipBehavior), new FrameworkPropertyMetadata((string)null));

		[Description("Formatting of the header string.")]
		[Category("Common")]
		[DisplayName("Header StringFormat")]
		public string TitleStringFormat
		{
			get
			{
				return (string)GetValue(TitleStringFormatProperty);
			}
			set
			{
				SetValue(TitleStringFormatProperty, value);
			}
		}

		public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(ToolTipCategory), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(ToolTipCategory.None));

		[Bindable(true)]
		[Description("Severity and tooltip icon.")]
		[Category("Common")]
		[DisplayName("Category / Theme")]
		public ToolTipCategory Category
		{
			get
			{
				return (ToolTipCategory)GetValue(CategoryProperty);
			}
			set
			{
				SetValue(CategoryProperty, value);
			}
		}

		public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(300.0));

		[Bindable(true)]
		[Description("Maximum width of the popup (text will break to next line).")]
		[DisplayName("Max ToolTip Width")]
		public double MaxWidth
		{
			get
			{
				return (double)GetValue(MaxWidthProperty);
			}
			set
			{
				SetValue(MaxWidthProperty, value);
			}
		}

		public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(500, OnDelayChanged), ValidateDelay);

		private static bool ValidateDelay(object value)
		{
			var delay = (int)value;
			return delay >= 100;
		}

		private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var b = (ToolTipBehavior)d;
			b.timer.Interval = TimeSpan.FromMilliseconds(b.Delay);
		}

		[Description("Delay to show / hide tooltips.")]
		[Bindable(true)]
		[DisplayName("ToolTip Delay")]
		public int Delay
		{
			get
			{
				return (int)GetValue(DelayProperty);
			}
			set
			{
				SetValue(DelayProperty, value);
			}
		}

		public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register("IsInteractive", typeof(bool), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(true, OnIsInteractiveChanged));

		[Bindable(true)]
		[DisplayName("Clickable ToolTip")]
		[Description("Whether the ToolTip can be selected / clicked on.")]
		public bool IsInteractive
		{
			get
			{
				return (bool)GetValue(IsInteractiveProperty);
			}
			set
			{
				SetValue(IsInteractiveProperty, value);
			}
		}

		private static void OnIsInteractiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var target = (ToolTipBehavior)d;
			if (target.toolTip == null) return;

			target.toolTip.IsHitTestVisible = target.IsInteractive;
		}

		[Bindable(true)]
		[DisplayName("PinButtonVisibility")]
		[Description("Set PinButton Visibility")]
		public Visibility PinButtonVisibility
		{
			get
			{
				return (Visibility)GetValue(PinButtonVisibilityProperty);
			}
			set
			{
				SetValue(PinButtonVisibilityProperty, value);
			}
		}

		public static readonly DependencyProperty PinButtonVisibilityProperty = DependencyProperty.Register("PinButtonVisibility", typeof(Visibility), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Bindable(true)]
		[DisplayName("ContentMargin")]
		[Description("Set Inline Content Padding")]
		public Thickness ContentMargin
		{
			get
			{
				return (Thickness)GetValue(ContentMarginProperty);
			}
			set
			{
				SetValue(ContentMarginProperty, value);
			}
		}

		public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(new Thickness(5), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register("IsPinned", typeof(bool), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		[Bindable(true)]
		[DisplayName("IsPinned")]
		[Description("Tooltip stays open in main window.")]
		public bool IsPinned
		{
			get
			{
				return (bool)GetValue(IsPinnedProperty);
			}
			set
			{
				SetValue(IsPinnedProperty, value);
			}
		}

		[Bindable(true)]
		[DisplayName("IsSuperPinned")]
		[Description("Tooltip stays open all windows.")]
		public bool IsSuperPinned
		{
			get
			{
				return (bool)GetValue(IsSuperPinnedProperty);
			}
			set
			{
				SetValue(IsSuperPinnedProperty, value);
			}
		}

		public static readonly DependencyProperty IsSuperPinnedProperty = DependencyProperty.Register("IsSuperPinned", typeof(bool), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		[Bindable(true)]
		[DisplayName("IsMovable")]
		[Description("Tooltip can be moved.")]
		public bool IsMovable
		{
			get
			{
				return (bool)GetValue(IsMovableProperty);
			}
			set
			{
				SetValue(IsMovableProperty, value);
			}
		}

		public static readonly DependencyProperty IsMovableProperty = DependencyProperty.Register("IsMovable", typeof(bool), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(true, OnIsEnabledChanged));

		[Bindable(true)]
		[Description("Optionally deactivates ToolTips.")]
		public bool IsEnabled
		{
			get
			{
				return (bool)GetValue(IsEnabledProperty);
			}
			set
			{
				SetValue(IsEnabledProperty, value);
			}
		}

		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var behavior = (ToolTipBehavior)d;
			if (!behavior.IsEnabled)
			{
				behavior.HideAndClose();
			}
		}

		[Bindable(true)]
		[Description("Set popup position.")]
		public PlacementMode PopupMousePosition
		{
			get
			{
				return (PlacementMode)GetValue(PopupMousePositionProperty);
			}
			set
			{
				SetValue(PopupMousePositionProperty, value);
			}
		}

		public static readonly DependencyProperty PopupMousePositionProperty = DependencyProperty.Register("PopupMousePosition", typeof(PlacementMode), typeof(ToolTipBehavior), new FrameworkPropertyMetadata(PlacementMode.Mouse, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public ToolTipBehavior()
		{
			timer = new DispatcherTimer(TimeSpan.FromMilliseconds(Delay), DispatcherPriority.Normal, OnTimerElapsed, Dispatcher)
			{
				IsEnabled = false
			};
		}

		private Point _initialMousePosition;
		private bool _isDragging;
		private void InitControls()
		{

			popup = new Popup
			{
				PlacementTarget = AssociatedObject,
				Placement = PopupMousePosition,
				HorizontalOffset = 5,
				VerticalOffset = 5,
				AllowsTransparency = true
			};

			popup.MouseEnter += OnPopupMouseEnter;
			popup.MouseLeave += OnPopupMouseLeave;

			if (IsMovable)
			{
				popup.MouseLeftButtonDown += (f3, g3) =>
				{
					var element = f3 as FrameworkElement;
					_initialMousePosition = g3.GetPosition(null);
					element.CaptureMouse();
					_isDragging = true;
					g3.Handled = true;
				};
				popup.MouseMove += (f2, g2) =>
				{
					if (_isDragging)
					{
						var currentPoint = g2.GetPosition(null);
						popup.HorizontalOffset += (currentPoint.X - _initialMousePosition.X);
						popup.VerticalOffset += (currentPoint.Y - _initialMousePosition.Y);
					}
				};
				popup.MouseLeftButtonUp += (f, g) =>
				{
					if (_isDragging)
					{
						var element = f as FrameworkElement;
						element.ReleaseMouseCapture();
						_isDragging = false;
						g.Handled = true;
					}
				};
			}

			var binding = new Binding
			{
				Path = new PropertyPath(FrameworkElement.DataContextProperty),
				Mode = BindingMode.OneWay,
				Source = AssociatedObject
			};
			BindingOperations.SetBinding(popup, FrameworkElement.DataContextProperty, binding);

			toolTip = Content as HeaderedToolTip ?? CreateWrapperControl();

			popup.Child = toolTip;
			toolTip.IsHitTestVisible = IsInteractive;
			toolTip.Tag = this;
			toolTip.ApplyTemplate();

			VisualStateManager.GoToState(toolTip, "Closed", false);
		}

		private HeaderedToolTip CreateWrapperControl()
		{
			var tt = new HeaderedToolTip();
			CreateBinding(ContentProperty, ContentControl.ContentProperty, tt);

			CreateBinding(ContentStringFormatProperty, ContentControl.ContentStringFormatProperty, tt);

			CreateBinding(TitleProperty, HeaderedContentControl.HeaderProperty, tt);

			CreateBinding(TitleStringFormatProperty, HeaderedContentControl.HeaderStringFormatProperty, tt);

			CreateBinding(CategoryProperty, HeaderedToolTip.CategoryProperty, tt);

			CreateBinding(MaxWidthProperty, FrameworkElement.MaxWidthProperty, tt);

			return tt;
		}

		private void CreateBinding(DependencyProperty behaviorProperty, DependencyProperty targetProperty, DependencyObject target)
		{
			var binding = new Binding
			{
				Path = new PropertyPath(behaviorProperty),
				Mode = BindingMode.OneWay,
				Source = this
			};
			BindingOperations.SetBinding(target, targetProperty, binding);
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.MouseEnter += OnMouseEnter;
			AssociatedObject.MouseLeave += OnMouseLeave;
		}

		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			if (!IsEnabled) return;

			if (popup != null && popup.IsOpen)
			{

				Schedule(() => { });
				VisualStateManager.GoToState(toolTip, "Showing", true);
			}
			else
			{
				Schedule(() =>
				{
					popup.IsOpen = true;

					VisualStateManager.GoToState(toolTip, "Showing", true);
				});
			}
		}

		private void OnPopupMouseEnter(object sender, MouseEventArgs e)
		{
			timer.Stop();
			VisualStateManager.GoToState(toolTip, "Active", true);

			var parentWindow = Window.GetWindow(AssociatedObject);
			if (parentWindow != null)
			{
				parentWindow.Deactivated += OnParentWindowDeactivated;
				parentWindow.Closed += ParentWindow_Closed;
			}
		}

		private void ParentWindow_Closed(object sender, EventArgs e)
		{
			timer.Stop();
			var parentWindow = (Window)sender;
			parentWindow.Closed -= ParentWindow_Closed;

			popup.IsOpen = false;
			VisualStateManager.GoToState(toolTip, "Closed", false);

		}

		private void OnParentWindowDeactivated(object sender, EventArgs e)
		{
			timer.Stop();

			var parentWindow = (Window)sender;
			parentWindow.Deactivated -= OnParentWindowDeactivated;
			if (IsSuperPinned)
			{
				popup.IsOpen = true;
			}
			else
			{
				popup.IsOpen = false;
				VisualStateManager.GoToState(toolTip, "Closed", false);

			}
		}

		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			HideAndClose();
		}

		private void HideAndClose()
		{
			if (popup == null) return;

			VisualStateManager.GoToState(toolTip, "Hiding", true);

			Schedule(() =>
			{
				VisualStateManager.GoToState(toolTip, "Closed", true);
				popup.IsOpen = false;
			});
		}

		private void OnPopupMouseLeave(object sender, MouseEventArgs e)
		{
			if (IsPinned || IsSuperPinned)
			{
				return;
			}
			var parentWindow = Window.GetWindow(AssociatedObject);
			if (parentWindow != null)
			{
				parentWindow.Deactivated -= OnParentWindowDeactivated;
			}

			Schedule(() =>
			{
				VisualStateManager.GoToState(toolTip, "Hiding", true);
				Schedule(() =>
				{
					popup.IsOpen = false;
				});
			});
		}

		private void Schedule(Action action)
		{
			lock (timer)
			{
				timer.Stop();
				if (popup == null) InitControls();

				timerAction = action;
				timer.Start();
			}
		}

		private void OnTimerElapsed(object sender, EventArgs eventArgs)
		{
			lock (timer)
			{
				timer.Stop();

				var action = timerAction;
				timerAction = null;

				if (action != null) action();
			}
		}
	}
}