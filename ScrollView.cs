using System;
using System.Collections.Generic;
using FinGameWorks.Scripts.Helpers;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace FinGameWorks.Scripts.Views
{
    public class ScrollView : StatefulWidget
    {
        public readonly Widget child;
        public readonly float MinScale;
        public readonly float MaxScale;
        public readonly float ContentSizeWidth;
        public readonly float ContentSizeHeight;
        
        public ScrollView(Widget child, float minScale = 0.5f, float maxScale = 3.0f, float contentSizeWidth = 2000, float contentSizeHeight = 2000, Key key = null) : base(key)
        {
            this.child = child;
            MinScale = minScale;
            MaxScale = maxScale;
            ContentSizeWidth = contentSizeWidth;
            ContentSizeHeight = contentSizeHeight;
        }

        public override State createState()
        {
            return new ScrollViewState();
        }
    }

    public class ScrollViewState : SingleTickerProviderStateMixin<ScrollView>
    {
        public Offset Offset = Offset.zero;
        public float Scale = 1;
        public Offset MoveEndVelocity = Offset.zero;
        public readonly GlobalKey ContentViewContainerKey = GlobalKey.key();

        private float aimedScale = 1;
        private Offset aimedOffset = Offset.zero;
        
        private Offset previousScrollPosition = Offset.zero;
        private Offset previousDragPosition = Offset.zero;
        private float previousScale = 1;
        private TimeSpan previousTime = TimeSpan.Zero;
        
        public override void initState()
        {
            base.initState();
            WidgetsBinding.instance.scheduleFrameCallback(FrameCallback);
        }

        private void FrameCallback(TimeSpan dur)
        {
            setState(() =>
            {
                Offset moveEndVelocityChangeSpeed = Offset.zero;
                Offset offsetChangeSpeed = Offset.zero;
                float scaleChangeSpeed = 0;
                MoveEndVelocity = MoveEndVelocity.DampTo(Offset.zero, ref moveEndVelocityChangeSpeed, 0.3f);
                aimedOffset += MoveEndVelocity / (1000.0f / (dur - previousTime).Milliseconds);
                postCheck();
                
                Offset = Offset.DampTo(aimedOffset, ref offsetChangeSpeed,0.3f);
                Scale = Mathf.SmoothDamp(Scale, aimedScale, ref scaleChangeSpeed, 0.3f);
                previousTime = dur;
            });
            WidgetsBinding.instance.scheduleFrameCallback(FrameCallback);
        }

        private void postCheck()
        {
            aimedScale = Mathf.Max(widget.MinScale, aimedScale);
            aimedScale = Mathf.Min(widget.MaxScale, aimedScale);
            
            if (widget.ContentSizeWidth * aimedScale < MediaQuery.of(context).size.width &&
                widget.ContentSizeHeight * aimedScale < MediaQuery.of(context).size.height)
            {
                aimedScale = Mathf.Max(MediaQuery.of(context).size.width / widget.ContentSizeWidth,
                    MediaQuery.of(context).size.height / widget.ContentSizeHeight);
                aimedOffset = new Offset(0,0);
            }
            
            if (aimedOffset.dx > 0)
            {
                aimedOffset = new Offset(0, aimedOffset.dy);
            }

            if (aimedOffset.dy > 0)
            {
                aimedOffset = new Offset(aimedOffset.dx, 0);
            }

            if (widget.ContentSizeWidth * aimedScale + aimedOffset.dx < MediaQuery.of(context).size.width)
            {
                aimedOffset =
                    new Offset(MediaQuery.of(context).size.width - widget.ContentSizeWidth * aimedScale,
                        aimedOffset.dy);
            }

            if (widget.ContentSizeHeight * aimedScale + aimedOffset.dy < MediaQuery.of(context).size.height)
            {
                aimedOffset = new Offset(aimedOffset.dx,
                    MediaQuery.of(context).size.height - widget.ContentSizeHeight * aimedScale);
            }
        }

        private void scroll(Offset origin, float diff)
        {
            aimedScale += diff;
            Offset scaleOffsetDiff = (aimedOffset - previousScrollPosition) * (diff / aimedScale);
            aimedOffset += scaleOffsetDiff;
            previousScale = aimedScale;
            previousScrollPosition = origin;
        }

        private void move(Offset origin)
        {
            aimedOffset += origin - previousDragPosition;
            previousDragPosition = origin;
        }

        public override Widget build(BuildContext context)
        {
            return new GestureDetector
            (
                onLongPressDragUpdate: details =>
                {
                    Debug.Log("onLongPressDragUpdate");
                }, 
                onScaleStart: details =>
                {
                    MoveEndVelocity = Offset.zero;
                    previousScrollPosition = details.focalPoint;
                    previousDragPosition = details.focalPoint;
                },
                onScaleUpdate: details =>
                {
                    Debug.Log("onScaleUpdate");
                    MoveEndVelocity = Offset.zero;
                    float scaleDiff = previousScale * (details.scale - 1);
                    if (aimedScale >= widget.MaxScale && scaleDiff > 0)
                    {
                        scaleDiff = 0;
                    }

                    if (aimedScale <= widget.MinScale && scaleDiff < 0)
                    {
                        scaleDiff = 0;
                    }
                    scroll(details.focalPoint, scaleDiff);
                    move(details.focalPoint);
//                    postCheck();
                },
                onScaleEnd: details =>
                {
                    MoveEndVelocity = details.velocity.pixelsPerSecond;
                }, 
                onVerticalDragStart: details =>
                {
                    MoveEndVelocity = Offset.zero;
                    previousScrollPosition = details.globalPosition;
                    previousDragPosition = details.globalPosition;
                }, 
                onVerticalDragUpdate: details =>
                {
                    Debug.Log("onVerticalDragUpdate");
                    if (details.isScroll)
                    {
                        float scaleDiff = details.delta.dy / MediaQuery.of(context).size.height;
                        if (aimedScale >= widget.MaxScale && scaleDiff > 0)
                        {
                            scaleDiff = 0;
                        }
                        if (aimedScale <= widget.MinScale && scaleDiff < 0)
                        {
                            scaleDiff = 0;
                        }
                        if (Math.Abs(scaleDiff) > float.Epsilon)
                        {
                            scroll(details.globalPosition, scaleDiff);
                        }
//                        postCheck();  
                    }
                    else
                    {
                        move(details.globalPosition);
//                        postCheck();
                    }
                }, 
                onVerticalDragEnd: details =>
                {
                    MoveEndVelocity = details.velocity.pixelsPerSecond;
                },
                child: new ClipRect
                (
                    clipBehavior: Clip.hardEdge,
                    child: new Stack
                    (
                        children: new List<Widget>
                        {
                            new Positioned
                            (
                                left: Offset.dx,
                                top: Offset.dy,
                                child: Transform.scale
                                (
                                    scale: Scale,
                                    child: new Container(
                                        key: ContentViewContainerKey,
                                        width: widget.ContentSizeWidth,
                                        height: widget.ContentSizeHeight,
                                        child: widget.child
                                    ),
                                    alignment: Alignment.topLeft
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}