using System;
using System.Diagnostics;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CounterTextView
{
    public class TextViewCounter : TextView
    {
        private int fixedStop = -11111111;
        private int lastnumber;
        private bool isViewVisible=true,futureDone;
        private long elapsedSecond;
        private long MillisUntilFinished;
        public event FinishEvent Finish;
        CounterTextViewModel counterTextViewModel;

        public bool Repeat { get { return counterTextViewModel.repeat; } set {
                counterTextViewModel.repeat = value;
                SetText();
            }
        }

        public bool IsViewVisible
        {
            get { return isViewVisible; }
            set
            {
                isViewVisible = value;
                if (!isViewVisible)
                {
                    CloseTimer();
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }
                else
                {
                    if (stopwatch != null)
                    {
                        elapsedSecond = stopwatch.ElapsedMilliseconds;
                        int countgap = (int)(elapsedSecond / 1000);
                        lastnumber = lastnumber + (countgap * counterTextViewModel.progression);
                        counterTextViewModel.future = (int)(MillisUntilFinished - elapsedSecond);
                    }
                    CloseStopWatch();
                    SetTextSub();
                }
            }
        }

        public int Start
        {
            get { return counterTextViewModel.start; }
            set
            {
                counterTextViewModel.start = value;
                SetText();
            }
        }

        public int Stop
        {
            get { return counterTextViewModel.stop; }
            set
            {
                counterTextViewModel.stop = value;
                SetText();
            }
        }

        public int Progression
        {
            get { return counterTextViewModel.progression; }
            set
            {
                counterTextViewModel.progression = value;
                SetText();
            }
        }

        public int Interval
        {
            get { return counterTextViewModel.interval; }
            set
            {
                counterTextViewModel.interval = value;
                SetText();
            }
        }

        public int Future
        {
            get { return counterTextViewModel.future; }
            set
            {
                counterTextViewModel.future = value;
                futureDone = false;
                if (counterTextViewModel.future >0)
                    futureDone = true;
                SetText();
            }
        }

        public string StartText
        {
            get { return counterTextViewModel.StartText; }
            set
            {
                counterTextViewModel.StartText = value;
                SetText();
            }
        }

        public string StopText
        {
            get { return counterTextViewModel.StopText; }
            set
            {
                counterTextViewModel.StopText = value;
            }
        }

        public CounterTextViewModel CounterTextViewModel
        {
            get { return counterTextViewModel; }
            set
            {
                counterTextViewModel = value;
                SetText();
            }
        }


        public TextViewCounter(Context context) : base(context)
        {
            init();
        }

        public TextViewCounter(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs);
        }

        public TextViewCounter(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs);
        }

        public TextViewCounter(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs);
        }

        protected TextViewCounter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            init();
        }

        private void ConstructModel()
        {
            counterTextViewModel = new CounterTextViewModel();
            counterTextViewModel.StopText = string.Empty;
            counterTextViewModel.StartText = string.Empty;
            counterTextViewModel.progression = 1;
            counterTextViewModel.interval = 1000;
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            ConstructModel();
            counterTextViewModel.stop = fixedStop;
            elapsedSecond = fixedStop;
            TypedArray ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.CounterTextView, 0, 0);
            try
            {
                counterTextViewModel.progression = ta.GetInt(Resource.Styleable.CounterTextView_progression,1);
                counterTextViewModel.stop = ta.GetInt(Resource.Styleable.CounterTextView_stop, -11111111);
                counterTextViewModel.StopText = ta.GetString(Resource.Styleable.CounterTextView_stopText);
                counterTextViewModel.start = ta.GetInt(Resource.Styleable.CounterTextView_start, 0);
                counterTextViewModel.StartText = ta.GetString(Resource.Styleable.CounterTextView_startText);
                counterTextViewModel.interval = ta.GetInt(Resource.Styleable.CounterTextView_interval, 1000);
                counterTextViewModel.future = ta.GetInt(Resource.Styleable.CounterTextView_future, 0);
                counterTextViewModel.repeat = ta.GetBoolean(Resource.Styleable.CounterTextView_repeat, false);
                if (counterTextViewModel.future > 0)
                    futureDone = true;
            }
            finally
            {
                ta.Recycle();
            }
            lastnumber = counterTextViewModel.start; ;
            SetText();
        }

        

        private void init()
        {
            ConstructModel();
            counterTextViewModel.stop = fixedStop;
            elapsedSecond = fixedStop;
            SetText();
        }

        CountDown CountDownTimer;
        Stopwatch stopwatch;
        private void SetText()
        {
            CloseTimer();
            try
            {
                Text = string.IsNullOrEmpty(counterTextViewModel.StartText) ? counterTextViewModel.start.ToString() : counterTextViewModel.StartText;
                lastnumber = counterTextViewModel.start;
                SetFuture();
                SetTextSub();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CounterTextView:TextViewCounter:SetText:" + ex.Message);
            }
        }


        private void SetTextSub()
        {
            if (Visibility == Android.Views.ViewStates.Visible && counterTextViewModel.future > 0)
            {
                CloseTimer();
                counterTextViewModel.CounterEnd = false;
                CountDownTimer = new CountDown(counterTextViewModel.future, counterTextViewModel.interval);
                CountDownTimer.Tick += (long millisUntilFinished) => {
                    try
                    {
                        MillisUntilFinished = millisUntilFinished;
                        lastnumber = lastnumber + counterTextViewModel.progression;
                        Text = lastnumber.ToString();
                        if (lastnumber == counterTextViewModel.stop)
                        {
                            SendEvent();
                            StopCountDown();
                            Text = !string.IsNullOrEmpty(counterTextViewModel.StopText) ? counterTextViewModel.StopText : counterTextViewModel.stop == fixedStop ? lastnumber.ToString() : counterTextViewModel.stop.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("CounterTextView:TextViewCounter:SetText:" + ex.Message);
                    }
                };
                CountDownTimer.Finish += () => {
                    try
                    {
                        SendEvent();
                        if (!counterTextViewModel.repeat)
                            Text = !string.IsNullOrEmpty(counterTextViewModel.StopText) ? counterTextViewModel.StopText : counterTextViewModel.stop == fixedStop ? lastnumber.ToString() : counterTextViewModel.stop.ToString();
                        else
                        {
                            SetText();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("CounterTextView:TextViewCounter:SetText:" + ex.Message);
                    }
                };
                CountDownTimer.Start();
            }
        }

        public override ViewStates Visibility { get => base.Visibility; set {
                base.Visibility = value;
                CloseTimer();
            }
        }

        private void CloseTimer()
        {
            try
            {
                CountDownTimer?.Cancel();
                CountDownTimer?.Dispose();
                CountDownTimer = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CounterTextView:TextViewCounter:CloseTimer:" + ex.Message);
            }
        }

        private void CloseStopWatch()
        {
            try
            {
                stopwatch?.Stop();
                stopwatch = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CounterTextView:TextViewCounter:CloseTimer:" + ex.Message);
            }
        }

        private void SetFuture()
        {
            if (counterTextViewModel.future < 0 || !futureDone)
            {
                int diff = Math.Abs(counterTextViewModel.start - (counterTextViewModel.stop == fixedStop ? 0 : counterTextViewModel.stop));
                counterTextViewModel.future = diff * 1000;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            StopCountDown();
        }

        public void StopCountDown()
        {
            if (!isViewVisible && stopwatch!=null)
            {
                elapsedSecond = stopwatch.ElapsedMilliseconds;
                int countgap = (int)(elapsedSecond / 1000);
                if (counterTextViewModel != null)
                {
                    counterTextViewModel.start = lastnumber + (countgap * counterTextViewModel.progression);
                    counterTextViewModel.future = (int)(MillisUntilFinished - elapsedSecond);
                }
            }
            CloseStopWatch();
            CloseTimer();
        }

        private void SendEvent()
        {
            counterTextViewModel.CounterEnd = true;
            Finish?.Invoke();
        }
    }

    internal delegate void TickEvent(long millisUntilFinished);
    public delegate void FinishEvent();
    internal class CountDown : Android.OS.CountDownTimer
    {
        public event TickEvent Tick;
        public event FinishEvent Finish;

        public CountDown(long totaltime, long interval): base(totaltime, interval)
        {
        }

        public override void OnTick(long millisUntilFinished)
        {
            Tick?.Invoke(millisUntilFinished);
        }

        public override void OnFinish()
        {
            Finish?.Invoke();
        }
    }

    public class CounterTextViewModel
    {
        public int start { get; set; }
        public int stop { get; set; }
        public int interval { get; set; }
        public int future { get; set; }
        public int progression { get; set; }
        public bool repeat { get; set; }
        public string StartText { get; set; }
        public string StopText { get; set; }
        public bool CounterEnd { get; set; }
    }
}
