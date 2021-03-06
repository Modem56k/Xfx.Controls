﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xfx;
using Xfx.Controls.Droid.Forms.Internals;
using Xfx.Controls.Droid.Renderers;
using Xfx.Controls.Droid.XfxComboBox;
using Resource = Android.Resource;

[assembly: ExportRenderer(typeof(XfxComboBox), typeof(XfxComboBoxRendererDroid))]

namespace Xfx.Controls.Droid.Renderers
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class XfxComboBoxRendererDroid : XfxEntryRendererDroid
    {
        public XfxComboBoxRendererDroid(Context context) : base(context)
        {
        }

        private InstantAutoCompleteTextView AutoComplete => (InstantAutoCompleteTextView)Control?.EditText;

        protected override TextInputLayout CreateNativeControl()
        {
            var textInputLayout = new TextInputLayout(Context);
            var autoComplete = new InstantAutoCompleteTextView(Context)
            {
                SupportBackgroundTintList = ColorStateList.ValueOf(GetPlaceholderColor())
            };
            textInputLayout.AddView(autoComplete);
            return textInputLayout;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<XfxEntry> e)
        {
            base.OnElementChanged(e);

            if (Element == null || Control == null || Control.Handle == IntPtr.Zero || EditText == null || EditText.Handle == IntPtr.Zero || AutoComplete == null || AutoComplete.Handle == IntPtr.Zero)
                return;

            if (e.OldElement != null)
            {
                // unsubscribe
                AutoComplete.ItemClick -= AutoCompleteOnItemSelected;

                if (e.OldElement is Xfx.XfxComboBox elm)
                    elm.CollectionChanged -= ItemsSourceCollectionChanged;
            }

            if (e.NewElement != null)
            {
                // subscribe
                SetItemsSource();
                SetThreshold();
                KillPassword();
                AutoComplete.ItemClick += AutoCompleteOnItemSelected;

                if (e.NewElement is Xfx.XfxComboBox elm)
                {
                    elm.CollectionChanged += ItemsSourceCollectionChanged;
                    AutoComplete.SetOpenOnFocus(elm.OpenOnFocus);
                    AutoComplete.ShowIfEmpty = elm.ShowIfEmpty;
                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Element == null || Control == null || Control.Handle == IntPtr.Zero || EditText == null || EditText.Handle == IntPtr.Zero || AutoComplete == null || AutoComplete.Handle == IntPtr.Zero)
                return;

            if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
                KillPassword();
            if (e.PropertyName == Xfx.XfxComboBox.ItemsSourceProperty.PropertyName)
                SetItemsSource();
            else if (e.PropertyName == Xfx.XfxComboBox.ThresholdProperty.PropertyName)
                SetThreshold();
        }

        private void AutoCompleteOnItemSelected(object sender, AdapterView.ItemClickEventArgs args)
        {
            // Method is called via event subscription which could result to a call to a disposed object.
            if (Element == null || Control == null || Control.Handle == IntPtr.Zero || EditText == null || EditText.Handle == IntPtr.Zero || AutoComplete == null || AutoComplete.Handle == IntPtr.Zero)
                return;

            var view = (AutoCompleteTextView)sender;
            var selectedItemArgs = new XfxSelectedItemChangedEventArgs(view.Text, args.Position);
            var element = (Xfx.XfxComboBox)Element;
            element.OnItemSelectedInternal(Element, selectedItemArgs);
            HideKeyboard();
        }

        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            // Method is called via event subscription which could result to a call to a disposed object.
            if (Element == null || Control == null || Control.Handle == IntPtr.Zero || EditText == null || EditText.Handle == IntPtr.Zero || AutoComplete == null || AutoComplete.Handle == IntPtr.Zero)
                return;

            var element = (Xfx.XfxComboBox)Element;
            ResetAdapter(element);
        }

        private void KillPassword()
        {
            if (Element.IsPassword)
                throw new NotImplementedException("Cannot set IsPassword on a XfxComboBox");
        }

        private void ResetAdapter(Xfx.XfxComboBox element)
        {
            var adapter = new XfxComboBoxArrayAdapter(Context,
                Resource.Layout.SimpleDropDownItem1Line,
                element.ItemsSource.ToList(),
                element.SortingAlgorithm);
            AutoComplete.Adapter = adapter;
            adapter.NotifyDataSetChanged();
        }

        private void SetItemsSource()
        {
            var element = (Xfx.XfxComboBox)Element;
            if (element.ItemsSource == null) return;

            ResetAdapter(element);
        }

        private void SetThreshold()
        {
            var element = (Xfx.XfxComboBox)Element;
            AutoComplete.Threshold = element.Threshold;
        }
    }
}