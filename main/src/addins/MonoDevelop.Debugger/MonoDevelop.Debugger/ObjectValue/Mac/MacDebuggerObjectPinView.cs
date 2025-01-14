//
// MacDebuggerObjectPinView.cs
//
// Author:
//       Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corp.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using AppKit;

using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.Debugger
{
	class MacDebuggerObjectPinView : MacDebuggerObjectCellViewBase
	{
		static readonly NSImage unpinnedImage = GetImage ("md-pin-up", Gtk.IconSize.Menu);
		static readonly NSImage pinnedImage = GetImage ("md-pin-down", Gtk.IconSize.Menu);
		static readonly NSImage liveUpdateOnImage = GetImage ("md-live", Gtk.IconSize.Menu);
		static readonly NSImage liveUpdateOffImage = GetImage ("md-live", Gtk.IconSize.Menu, 0.5);
		static readonly NSImage none = GetImage ("md-empty", Gtk.IconSize.Menu);
		public const int MinWidth = MarginSize + 16 + MarginSize;
		public const int MaxWidth = MarginSize + 16 + RowCellSpacing + 16 + MarginSize;
		bool disposed;
		bool pinned;

		public MacDebuggerObjectPinView (MacObjectValueTreeView treeView) : base (treeView, "pin")
		{
			PinButton = new NSButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BezelStyle = NSBezelStyle.Inline,
				Image = none,
				Bordered = false,
			};
			PinButton.AccessibilityTitle = GettextCatalog.GetString ("Pin to the editor");
			PinButton.Activated += OnPinButtonClicked;
			AddSubview (PinButton);

			LiveUpdateButton = new NSButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				BezelStyle = NSBezelStyle.Inline,
				Image = liveUpdateOffImage,
				Bordered = false
			};
			LiveUpdateButton.AccessibilityTitle = GettextCatalog.GetString ("Refresh value");
			LiveUpdateButton.Activated += OnLiveUpdateButtonClicked;
			AddSubview (LiveUpdateButton);

			PinButton.CenterYAnchor.ConstraintEqualToAnchor (CenterYAnchor).Active = true;
			PinButton.LeadingAnchor.ConstraintEqualToAnchor (LeadingAnchor, MarginSize).Active = true;
			PinButton.WidthAnchor.ConstraintEqualToConstant (ImageSize).Active = true;
			PinButton.HeightAnchor.ConstraintEqualToConstant (ImageSize).Active = true;

			LiveUpdateButton.CenterYAnchor.ConstraintEqualToAnchor (CenterYAnchor).Active = true;
			LiveUpdateButton.LeadingAnchor.ConstraintEqualToAnchor (PinButton.TrailingAnchor, RowCellSpacing).Active = true;
			LiveUpdateButton.TrailingAnchor.ConstraintEqualToAnchor (TrailingAnchor, -MarginSize).Active = true;
			LiveUpdateButton.WidthAnchor.ConstraintEqualToConstant (ImageSize).Active = true;
			LiveUpdateButton.HeightAnchor.ConstraintEqualToConstant (ImageSize).Active = true;
		}

		public MacDebuggerObjectPinView (IntPtr handle) : base (handle)
		{
			OptimalWidth = MaxWidth;
		}

		public NSButton PinButton {
			get; private set;
		}

		public NSButton LiveUpdateButton {
			get; private set;
		}

		protected override void UpdateContents ()
		{
			if (Node == null)
				return;

			if (TreeView.PinnedWatch != null && Node.Parent == TreeView.Controller.Root) {
				PinButton.Image = pinnedImage;
				pinned = true;
			} else {
				PinButton.Image = none;
				pinned = false;
			}

			if (pinned) {
				if (TreeView.PinnedWatch.LiveUpdate)
					LiveUpdateButton.Image = liveUpdateOnImage;
				else
					LiveUpdateButton.Image = liveUpdateOffImage;
			} else {
				LiveUpdateButton.Image = none;
			}
		}

		void OnPinButtonClicked (object sender, EventArgs e)
		{
			if (pinned) {
				TreeView.Unpin (Node);
			} else {
				TreeView.Pin (Node);
			}
		}

		void OnLiveUpdateButtonClicked (object sender, EventArgs e)
		{
			if (pinned) {
				DebuggingService.SetLiveUpdateMode (TreeView.PinnedWatch, !TreeView.PinnedWatch.LiveUpdate);
				Refresh ();
			}
		}

		public void SetMouseHover (bool hover)
		{
			if (pinned)
				return;

			PinButton.Image = hover || IdeServices.DesktopService.AccessibilityInUse ? unpinnedImage : none;
			SetNeedsDisplayInRect (PinButton.Frame);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				LiveUpdateButton.Activated -= OnLiveUpdateButtonClicked;
				PinButton.Activated -= OnPinButtonClicked;
				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}
