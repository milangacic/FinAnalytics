using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static ChartControls.EventManager;
using Point = System.Windows.Point;

namespace ChartControls {

    internal class EventManager : IDisposable {

        #region Enums        

        internal enum EventType { None, MouseDown, MouseUp, MouseMove, MouseDrag, MouseWheel, MouseEnter, MouseLeave, KeyDown, KeyUp }

        #endregion

        #region Classes

        internal class Event {

            #region Properties

            public EventType Type { get; private set; }
            public int Timestamp { get; private set; }
            public Point Position { get; private set; }
            public int Delta { get; private set; }
            public Key? Key { get; private set; }
            public bool IsRepeat { get; private set; }
            public bool IsToggle { get; private set; }
            public KeyStates? KeyState { get; private set; }
            public MouseButton? Button { get; private set; }
            public MouseButtonState? ButtonState { get; private set; }
            public MouseButtonState? LeftButtonState { get; private set; }
            public MouseButtonState? RightButtonState { get; private set; }
            public MouseButtonState? MiddleButtonState { get; private set; }
            public Vector Movement { get; private set; }
            public int Clicks { get; private set; }

            #endregion

            #region Public methods            

            public static Event CreateMouseDownEvent(IInputElement sender, MouseButtonEventArgs args) {
                return CreateMouseButtonEvent(EventType.MouseDown, args.Timestamp, args.GetPosition(sender), args.ChangedButton, args.ButtonState, args.ClickCount, args.LeftButton, args.RightButton, args.MiddleButton);
            }

            public static Event CreateMouseDownEvent(int timestamp, Point position, MouseButton btn, MouseButtonState? state, int clicks = 1, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return CreateMouseButtonEvent(EventType.MouseDown, timestamp, position, btn, state, clicks, leftbtn, rightbtn, middlebtn);
            }

            public static Event CreateMouseUpEvent(IInputElement sender, MouseButtonEventArgs args) {
                return CreateMouseButtonEvent(EventType.MouseUp, args.Timestamp, args.GetPosition(sender), args.ChangedButton, args.ButtonState, args.ClickCount, args.LeftButton, args.RightButton, args.MiddleButton);
            }

            public static Event CreateMouseUpEvent(int timestamp, Point position, MouseButton btn, MouseButtonState state, int clicks = 1, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return CreateMouseButtonEvent(EventType.MouseUp, timestamp, position, btn, state, clicks, leftbtn, rightbtn, middlebtn);
            }

            public static Event CreateMouseMoveEvent(IInputElement sender, MouseEventArgs args) {
                return CreateMouseMoveEvent(args.Timestamp, args.GetPosition(sender), args.LeftButton, args.RightButton, args.MiddleButton);
            }

            public static Event CreateMouseMoveEvent(int timestamp, Point position, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return CreateMouseEvent(EventType.MouseMove, timestamp, position, leftbtn, rightbtn, middlebtn);
            }

            public static Event CreateMouseWheelEvent(IInputElement sender, MouseWheelEventArgs args) {
                return CreateMouseWheelEvent(args.Timestamp, args.Delta);
            }

            public static Event CreateMouseWheelEvent(int timestamp, int delta) {
                return new Event(EventType.MouseWheel, timestamp) { Delta = delta };
            }

            public static Event CreateMouseEnterEvent(IInputElement sender, MouseEventArgs args) {
                return CreateMouseEnterEvent(args.Timestamp, args.GetPosition(sender), args.LeftButton, args.RightButton, args.MiddleButton);
            }

            public static Event CreateMouseEnterEvent(int timestamp, Point position, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return new Event(EventType.MouseEnter, timestamp) { Position = position, LeftButtonState = leftbtn, RightButtonState = rightbtn, MiddleButtonState = middlebtn };
            }

            public static Event CreateMouseLeaveEvent(IInputElement sender, MouseEventArgs args) {
                return CreateMouseLeaveEvent(args.Timestamp, args.GetPosition(sender), args.LeftButton, args.RightButton, args.MiddleButton);
            }

            public static Event CreateMouseLeaveEvent(int timestamp, Point position, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return new Event(EventType.MouseLeave, timestamp) { Position = position, LeftButtonState = leftbtn, RightButtonState = rightbtn, MiddleButtonState = middlebtn };
            }

            public static Event CreateMouseDragEvent(int timestamp, Point position, Vector movement, MouseButton btn, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return new Event(EventType.MouseDrag, timestamp) { Position = position, Button = btn, LeftButtonState = leftbtn, RightButtonState = rightbtn, MiddleButtonState = middlebtn, Movement = movement };
            }

            public static Event CreateKeyDownEvent(IInputElement sender, KeyEventArgs args) {
                return CreateKeyDownEvent(args.Timestamp, args.Key, args.KeyStates, args.IsRepeat, args.IsToggled);
            }

            public static Event CreateKeyDownEvent(int timestamp, Key key, KeyStates keystate, bool isrepeat = false, bool istoggle = false) {
                return CreateKeyEvent(EventType.KeyDown, timestamp, key, keystate, isrepeat, istoggle);
            }

            public static Event CreateKeyUpEvent(IInputElement sender, KeyEventArgs args) {
                return CreateKeyUpEvent(args.Timestamp, args.Key, args.KeyStates, args.IsRepeat, args.IsToggled);
            }

            public static Event CreateKeyUpEvent(int timestamp, Key key, KeyStates keystate, bool isrepeat = false, bool istoggle = false) {
                return CreateKeyEvent(EventType.KeyUp, timestamp, key, keystate, isrepeat, istoggle);
            }

            public bool IsPressed(MouseButton btn) {
                if (Button == btn) return ButtonState == MouseButtonState.Pressed;
                return btn == MouseButton.Left ? LeftButtonState == MouseButtonState.Pressed : (btn == MouseButton.Right && RightButtonState == MouseButtonState.Pressed);
            }

            public static bool IsPressed(MouseEventArgs args, MouseButton btn) {
                return btn == MouseButton.Left ? args.LeftButton == MouseButtonState.Pressed : (btn == MouseButton.Right && args.RightButton == MouseButtonState.Pressed);
            }

            public bool IsReleased(MouseButton btn) {
                if (Button == btn) return ButtonState == MouseButtonState.Released;
                return btn == MouseButton.Left ? LeftButtonState == MouseButtonState.Released : (btn == MouseButton.Right && RightButtonState == MouseButtonState.Released);
            }

            public static bool IsReleased(MouseEventArgs args, MouseButton btn) {
                return btn == MouseButton.Left ? args.LeftButton == MouseButtonState.Released : (btn == MouseButton.Right && args.RightButton == MouseButtonState.Released);
            }

            public bool IsUp(Key key) {
                return Key == key && (KeyState & KeyStates.Down) == 0;
            }

            public bool IsDown(Key key) {
                return Key == key && (KeyState & KeyStates.Down) != 0;
            }

            public override string ToString() {
                switch (Type) {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                        return string.Format("Type {0}, Timestamp {1}, Button {2}, Left Button State {3}, Right Button State {4}, Positon {5}", Type, Timestamp, Button, LeftButtonState, RightButtonState, Position);
                    case EventType.MouseDrag:
                        return string.Format("Type {0}, Timestamp {1}, Button {2}, Movement {3}, Positon {4}", Type, Timestamp, Button, Movement, Position);
                    case EventType.MouseWheel:
                        return string.Format("Type {0}, Timestamp {1}, Delta {2}", Type, Timestamp, Delta);
                    case EventType.MouseLeave:
                        return string.Format("Type {0}, Timestamp {1}", Type, Timestamp);
                    case EventType.MouseEnter:
                        return string.Format("Type {0}, Timestamp {1}", Type, Timestamp);
                    case EventType.KeyDown:
                    case EventType.KeyUp:
                        return string.Format("Type {0}, Timestamp {1}, Key {2}, State {3}", Type, Timestamp, Key, KeyState);
                    case EventType.None:
                        break;
                    default:
                        return string.Format("Type {0}, Timestamp {1}", Type, Timestamp);
                }
                return string.Format("Type {0}, Timestamp {1}", Type, Timestamp);
            }

            #endregion

            #region Private methods

            private Event(EventType type, int timestamp) {
                Type = type;
                Timestamp = timestamp;
                Position = default;
                IsToggle = false;
                IsRepeat = false;
                Key = null;
                KeyState = null;
                Button = null;
                ButtonState = null;
                Delta = 0;
                Movement = default;
                LeftButtonState = null;
                RightButtonState = null;
                MiddleButtonState = null;                
            }

            private static Event CreateMouseButtonEvent(EventType type, int timestamp, Point position, MouseButton btn, MouseButtonState? state, int clicks = 1, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                if (btn == MouseButton.Left) leftbtn = state;
                if (btn == MouseButton.Right) rightbtn = state;
                return new Event(type, timestamp) { Position = position, Button = btn, ButtonState = state, LeftButtonState = leftbtn, RightButtonState = rightbtn, Clicks = clicks };
            }

            private static Event CreateEvent(EventType type, int timestamp) {
                return new Event(type, timestamp);
            }

            private static Event CreateKeyEvent(EventType type, int timestamp, Key key, KeyStates keystate, bool isrepeat = false, bool istoggle = false) {
                return new Event(type, timestamp) { Key = key, KeyState = keystate, IsRepeat = isrepeat, IsToggle = istoggle };
            }

            private static Event CreateMouseEvent(EventType type, int timestamp, Point position, MouseButtonState? leftbtn = MouseButtonState.Released, MouseButtonState? rightbtn = MouseButtonState.Released, MouseButtonState? middlebtn = MouseButtonState.Released) {
                return new Event(type, timestamp) { Position = position, LeftButtonState = leftbtn, RightButtonState = rightbtn };
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly FinancialChartControl vChart;
        private readonly List<Event> vLastEvents;
        private Event vLastKeyEvent;
        private Event vLastMouseDownEvent;
        private Event vLastDragEvent;
        private DispatcherTimer vSingleClickTimer = new();
        private Action vSingleClickEvent;

        #endregion

        #region Constants

        private const int ThrowThreshold = 200;
        private const int DragThresholdTime = 500;
        private const int DragThresholdLength = 10;
        private const int DragSpeedThreshold = 2000;

        #endregion

        #region Events

        public event EventHandler<Point> Drag;
        public event EventHandler<Point> StopDragging;
        public event EventHandler<int> Throw;
        public event EventHandler<int> Wheel;
        public event EventHandler<Point> LeftClick;
        public event EventHandler<Point> RightClick;
        public event EventHandler<Point> DoubleLeftClick;
        public event EventHandler<Point> DoubleRightClick;
        public event EventHandler<int> LeftRightKey;
        public event EventHandler Leave;

        #endregion

        #region Public methods        

        public EventManager(FinancialChartControl chart) {
            vChart = chart;
            vLastEvents = new List<Event>();           
            AttachEventHandlers();
        }

        public void Dispose() {
            DetachEventHandlers();
        }

        #endregion

        #region Event Handler   
       
        internal void RaiseEvent(Event @event) {
            switch (@event.Type) {
                case EventType.MouseDown:
                    vSingleClickTimer.Stop();
                    AddEvent(@event);
                    break;
                case EventType.MouseUp:
                    if (@event.Button == MouseButton.Left && @event.ButtonState == MouseButtonState.Released) {
                        if (IsDragging()) {
                            if (IsThrowEvent(@event.Timestamp) && Throw != null) Throw.Invoke(vChart, GetDragSpeed());
                            else StopDragging?.Invoke(vChart, @event.Position);
                        } else if (IsClickEvent(@event, MouseButton.Left)) {
                            LeftClick?.Invoke(vChart, @event.Position);
                        } else if (IsDoubleClickEvent(@event, MouseButton.Left)) {
                            DoubleLeftClick?.Invoke(vChart, @event.Position);
                        }
                        AddEvent(@event);
                    } else if (@event.Button == MouseButton.Right && @event.ButtonState == MouseButtonState.Released) {
                        if (IsClickEvent(@event, MouseButton.Right)) {
                            RightClick?.Invoke(vChart, @event.Position);
                        } else if (IsDoubleClickEvent(@event, MouseButton.Left)) {
                            DoubleRightClick?.Invoke(vChart, @event.Position);
                        }
                        AddEvent(@event);
                    }
                    break;
                case EventType.MouseEnter:
                    if (IsDragging() && !IsDraggingEvent(@event, MouseButton.Left)) {
                        vLastDragEvent = null;
                    }
                    AddEvent(@event); 
                    break;
                case EventType.MouseLeave:
                    Leave?.Invoke(vChart, null);
                    if (IsDragging()) {
                        StopDragging?.Invoke(vChart, @event.Position);
                        AddEvent(@event);
                    }                    
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    if (@event.LeftButtonState == MouseButtonState.Pressed) {
                        if (IsDragEvent(@event, MouseButton.Left)) {
                            Drag?.Invoke(vChart, vLastMouseDownEvent.Position);
                            AddEvent(@event.Type == EventType.MouseDrag ? @event : Event.CreateMouseDragEvent(@event.Timestamp, @event.Position, @event.Position - vLastMouseDownEvent.Position, MouseButton.Left, @event.LeftButtonState, @event.RightButtonState, @event.MiddleButtonState));
                        } else if (IsDraggingEvent(@event, MouseButton.Left)) {
                            Drag?.Invoke(vChart, @event.Position);
                            AddEvent(@event.Type == EventType.MouseDrag ? @event : Event.CreateMouseDragEvent(@event.Timestamp, @event.Position, @event.Position - vLastDragEvent.Position, MouseButton.Left, @event.LeftButtonState, @event.RightButtonState, @event.MiddleButtonState));
                        } 
                    }
                    break;
                case EventType.MouseWheel:
                    if (!IsDragging()) {
                        int delta = GetWheelSpeed(@event.Delta);
                        Wheel?.Invoke(vChart, delta != 0 ? delta : Math.Sign(@event.Delta));
                        AddEvent(@event);
                    }
                    break;
                case EventType.KeyDown:
                    if (@event.Key == Key.Right || @event.Key == Key.Left) {
                        if (@event.IsRepeat) LeftRightKey?.Invoke(vChart, @event.Key == Key.Left ? -1 : 1);
                        AddEvent(@event);
                    }
                    break;
                case EventType.KeyUp:
                    if (IsKeyPressedEvent(@event, Key.Right) || IsKeyPressedEvent(@event, Key.Left)) {
                        LeftRightKey?.Invoke(vChart, @event.Key == Key.Left ? -1 : 1);
                        AddEvent(@event);
                    }
                    break;
                default: break;
            }
        }        

        private void OnKeyDown(object sender, KeyEventArgs e) {
            RaiseEvent(Event.CreateKeyDownEvent(vChart, e));
            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            RaiseEvent(Event.CreateKeyUpEvent(vChart, e));
            e.Handled = true;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            RaiseEvent(Event.CreateMouseWheelEvent(vChart, e));
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            RaiseEvent(Event.CreateMouseMoveEvent(vChart, e));
        }

        private void OnMouseEnter(object sender, MouseEventArgs e) {
            RaiseEvent(Event.CreateMouseEnterEvent(vChart, e));
        }

        private void OnMouseLeave(object sender, MouseEventArgs e) {
            RaiseEvent(Event.CreateMouseLeaveEvent(vChart, e));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            vSingleClickTimer.Stop();
            var @event = Event.CreateMouseDownEvent(vChart, e);
            if (e.ClickCount == 1) vChart.Focus();
            RaiseEvent(@event);                
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e) {
            var @event = Event.CreateMouseUpEvent(vChart, e);
            if (vLastMouseDownEvent?.Clicks < 2 && !IsDragging()) {
                DelayClickEvent(() => RaiseEvent(@event));
            } else {
                RaiseEvent(@event);
            }                       
        }

        private void OnLostFocus(object sender, System.Windows.RoutedEventArgs e) {
            vLastMouseDownEvent = null;
            vLastDragEvent = null;
            vLastKeyEvent = null;
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            vLastKeyEvent = null;
        }

        private void OnSingleClick(object sender, EventArgs args) {
            vSingleClickTimer.Stop();
            vSingleClickEvent();
        }        

        #endregion

        #region Private methods

        private void AttachEventHandlers() {
            vChart.MouseDown += OnMouseDown;
            vChart.MouseUp += OnMouseUp;
            vChart.MouseLeave += OnMouseLeave;
            vChart.LostMouseCapture += OnMouseLeave;
            vChart.MouseMove += OnMouseMove;
            vChart.MouseWheel += OnMouseWheel;
            vChart.KeyDown += OnKeyDown;
            vChart.KeyUp += OnKeyUp;
            vChart.LostKeyboardFocus += OnLostKeyboardFocus;
            vChart.LostFocus += OnLostFocus;
            vChart.MouseEnter += OnMouseEnter;
            vSingleClickTimer.Tick += OnSingleClick;
        }

        private void DetachEventHandlers() {
            vChart.MouseDown -= OnMouseDown;
            vChart.MouseUp -= OnMouseUp;
            vChart.MouseLeave -= OnMouseLeave;
            vChart.LostMouseCapture -= OnMouseLeave;
            vChart.MouseMove -= OnMouseMove;
            vChart.MouseWheel -= OnMouseWheel;
            vChart.KeyDown -= OnKeyDown;
            vChart.KeyUp -= OnKeyUp;
            vChart.LostKeyboardFocus -= OnLostKeyboardFocus;
            vChart.LostFocus -= OnLostFocus;
            vChart.MouseEnter -= OnMouseEnter;
            vSingleClickTimer.Tick -= OnSingleClick;
        }

        private void AddEvent(Event e) {
            vLastEvents.Add(e);
            if (vLastEvents.Count > 20) vLastEvents.RemoveAt(0);
            switch (e.Type) {
                case EventType.MouseDown:
                    vLastMouseDownEvent = e;
                    if (IsDragging(e.Button)) vLastDragEvent = null;
                    break;
                case EventType.MouseUp:
                    vLastMouseDownEvent = null;
                    if (IsDragging(e.Button)) vLastDragEvent = null;
                    break;
                case EventType.MouseEnter:
                    break;
                case EventType.MouseLeave:
                    break;
                case EventType.MouseDrag:
                    vLastDragEvent = e;
                    break;
                case EventType.MouseWheel:
                    break;
                case EventType.KeyDown:
                case EventType.KeyUp:
                    vLastKeyEvent = e;
                    break;
            }
            //Debug.WriteLine($"Event: {e}, Drag: {vLastDragEvent}, Mouse: {vLastMouseDownEvent}");
        }

        private void DelayClickEvent(Action action) {
            vSingleClickEvent = action;
            vSingleClickTimer.Interval = TimeSpan.FromMilliseconds(200);
            vSingleClickTimer.Start();
        }

        private bool IsKeyPressedEvent(Event @event, Key key) {
            if (vLastKeyEvent == null) return false;
            return @event.IsUp(key) && vLastKeyEvent.IsDown(key);
        }

        private bool IsClickEvent(Event @event, MouseButton btn) {
            if (@event.Type != EventType.MouseUp || @event.Button != btn) return false;
            if (vLastMouseDownEvent == null || vLastMouseDownEvent.Type != EventType.MouseDown || vLastMouseDownEvent.Clicks > 1 || vLastMouseDownEvent.Button != btn || IsDragging()) return false;
            if ((@event.Timestamp - vLastMouseDownEvent.Timestamp) >= DragThresholdTime || (@event.Position - vLastMouseDownEvent.Position).Length >= DragThresholdLength) return false;
            return vLastMouseDownEvent.IsPressed(btn) && @event.IsReleased(btn);
        }

        private bool IsDoubleClickEvent(Event @event, MouseButton btn) {
            if (@event.Type != EventType.MouseUp || @event.Button != btn) return false;
            if (vLastMouseDownEvent == null || vLastMouseDownEvent.Type != EventType.MouseDown || vLastMouseDownEvent.Clicks != 2 || vLastMouseDownEvent.Button != btn || IsDragging()) return false;
            if ((@event.Timestamp - vLastMouseDownEvent.Timestamp) >= DragThresholdTime || (@event.Position - vLastMouseDownEvent.Position).Length >= DragThresholdLength) return false;
            return vLastMouseDownEvent.IsPressed(btn) && @event.IsReleased(btn);
        }

        private bool IsDragEvent(Event @event, MouseButton btn) {
            if (!IsMouseDown(btn) || IsDragging()) return false;
            if ((@event.Timestamp - vLastMouseDownEvent.Timestamp) < DragThresholdTime && (@event.Position - vLastMouseDownEvent.Position).Length < DragThresholdLength) return false;
            return vLastMouseDownEvent.IsPressed(btn) && @event.IsPressed(btn);
        }

        private bool IsDraggingEvent(Event @event, MouseButton btn) {
            return IsDragging(btn) && @event.IsPressed(btn);
        }

        private bool IsThrowEvent(int timestamp) {
            if (!IsDragging()) return false;
            return (timestamp - vLastDragEvent.Timestamp) < ThrowThreshold;
        }

        private bool IsDragging(MouseButton? btn = null) {
            return vLastDragEvent != null && (btn == null || vLastDragEvent.Button == btn.Value);
        }

        private bool IsMouseDown(MouseButton? btn = null) {
            return vLastMouseDownEvent != null && vLastMouseDownEvent.Button == btn;
        }

        private int GetDragSpeed() {
            if (!IsDragging()) return 0;
            IEnumerable<Event> dragevents = vLastEvents.AsEnumerable().Reverse().TakeWhile(item => item.Type == EventType.MouseDrag && (vLastDragEvent.Timestamp - item.Timestamp) < DragSpeedThreshold && Math.Sign(item.Movement.X) == Math.Sign(vLastDragEvent.Movement.X));
            return (int)(10 * (vLastDragEvent.Position - dragevents.Last().Position).X / Math.Max(1, vLastDragEvent.Timestamp - dragevents.Last().Timestamp));
        }

        private static int GetWheelSpeed(int delta) {
            return SystemParameters.WheelScrollLines * delta / 120;
        }

        #endregion

    }
}