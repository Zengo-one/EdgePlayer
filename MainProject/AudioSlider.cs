using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MainProject
{
    public class AudioSlider : Slider
    {
        #region override Member 

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_thumb != null)
            {
                m_thumb.MouseEnter -= thumb_MouseEnter;
                m_thumb.LostMouseCapture -= thumb_LostMouseCapture;
            }

            m_thumb = (GetTemplateChild("PART_Track") as Track).Thumb;

            if (m_thumb != null)
            {
                m_thumb.MouseEnter += thumb_MouseEnter;
                m_thumb.LostMouseCapture += thumb_LostMouseCapture;
            }

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            m_isDownOnSlider = true;
        }
        #endregion
        #region Private handler 

        private void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && m_isDownOnSlider)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(
                    e.MouseDevice, e.Timestamp, MouseButton.Left);
                args.RoutedEvent = MouseLeftButtonDownEvent;
                (sender as Thumb).RaiseEvent(args);
            }
        }
        private void thumb_LostMouseCapture(object sender, EventArgs e)
        {
            m_isDownOnSlider = false;
        }
        #endregion

        #region Private Members 

        private Thumb m_thumb = null;
        private bool m_isDownOnSlider = false;
        #endregion
    }
}
