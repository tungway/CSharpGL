﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpGL;

namespace ShadowMapping
{
    public partial class FormDepthTexture : Form
    {
        Scene scene;
        private ActionList actionList;

        public FormDepthTexture()
        {
            InitializeComponent();

            this.Load += FormMain_Load;
            this.winGLCanvas1.OpenGLDraw += winGLCanvas1_OpenGLDraw;
            this.winGLCanvas1.Resize += winGLCanvas1_Resize;
            this.winGLCanvas1.MouseClick += winGLCanvas1_MouseClick;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            var rootElement = GetRootElement();
            //var teapot = ShadowMappingRenderer.Create();
            //var rootElement = teapot;

            var position = new vec3(0, 0, 1);
            var center = new vec3(0, 0, 0);
            var up = new vec3(0, 1, 0);
            var camera = new Camera(position, center, up, CameraType.Perspecitive, this.winGLCanvas1.Width, this.winGLCanvas1.Height);
            this.scene = new Scene(camera, this.winGLCanvas1)
            {
                RootElement = rootElement,
                ClearColor = Color.SkyBlue.ToVec4(),
            };

            Match(this.trvScene, scene.RootElement);
            this.trvScene.ExpandAll();

            var tansformAction = new TransformAction(rootElement);
            var shadowMappingAction = new ShadowMappingAction(rootElement);
            var renderAction = new RenderAction(rootElement, camera);
            var actionList = new ActionList();
            actionList.Add(tansformAction); actionList.Add(shadowMappingAction); actionList.Add(renderAction);
            this.actionList = actionList;
        }

        private void Match(TreeView treeView, SceneNodeBase rendererBase)
        {
            treeView.Nodes.Clear();
            var node = new TreeNode(rendererBase.ToString()) { Tag = rendererBase };
            treeView.Nodes.Add(node);
            Match(node, rendererBase);
        }

        private void Match(TreeNode node, SceneNodeBase rendererBase)
        {
            foreach (var item in rendererBase.Children)
            {
                var child = new TreeNode(item.ToString()) { Tag = item };
                node.Nodes.Add(child);
                Match(child, item);
            }
        }

        private SceneNodeBase GetRootElement()
        {
            //int width = 600, height = 400;
            //var innerCamera = new Camera(new vec3(5, 5, 5), new vec3(0, 0, 0), new vec3(0, 1, 0), CameraType.Perspecitive, width, height);
            //(innerCamera as IPerspectiveViewCamera).Far = 50;
            //innerCamera.GetProjectionMatrix();
            //innerCamera.GetViewMatrix();
            var localLight = new SpotLight(new vec3(5, 5, 5), new vec3(0, 0, 0), 60, 1, 500) { Color = new vec3(1, 1, 1), };
            var lightContainer = new LightContainerNode(localLight);
            {
                {
                    var teapot = DepthTeapotRenderer.Create();
                    lightContainer.Children.Add(teapot);
                }
                {
                    var ground = GroundRenderer.Create();
                    ground.Color = Color.Gray.ToVec4();
                    ground.Scale *= 10;
                    ground.WorldPosition = new vec3(0, -3, 0);
                    lightContainer.Children.Add(ground);
                }
            }

            var rectangle = RectangleRenderer.Create();
            rectangle.TextureSource = localLight;

            var group = new GroupRenderer();
            group.Children.Add(lightContainer);
            group.Children.Add(rectangle);

            return group;
        }

        //private SceneNodeBase GetRootElement()
        //{
        //    int width = 600, height = 400;
        //    var innerCamera = new Camera(new vec3(0, 2, 5), new vec3(0, 0, 0), new vec3(0, 1, 0), CameraType.Perspecitive, width, height);
        //    (innerCamera as IPerspectiveViewCamera).Far = 50;
        //    IFramebufferProvider source = new DepthFramebufferProvider();
        //    var rtt = new RTTRenderer(width, height, innerCamera, source);
        //    {
        //        var teapot = DepthTextureRenderer.Create();
        //        rtt.Children.Add(teapot);
        //        var ground = GroundRenderer.Create(); ground.Color = Color.Gray.ToVec4(); ground.Scale *= 10; ground.WorldPosition = new vec3(0, -3, 0);
        //        rtt.Children.Add(ground);
        //    }

        //    var rectangle = RectangleRenderer.Create();
        //    rectangle.TextureSource = rtt;

        //    var group = new GroupRenderer();
        //    group.Children.Add(rtt);// rtt must be before rectangle.
        //    group.Children.Add(rectangle);
        //    //group.WorldPosition = new vec3(3, 0.5f, 0);// this looks nice.

        //    return group;
        //}

        private void winGLCanvas1_OpenGLDraw(object sender, PaintEventArgs e)
        {
            //this.scene.Render();
            this.actionList.Render();
        }

        void winGLCanvas1_Resize(object sender, EventArgs e)
        {
            this.scene.Camera.AspectRatio = ((float)this.winGLCanvas1.Width) / ((float)this.winGLCanvas1.Height);
        }

        /// <summary>
        /// click to pick and toggle the render wireframe state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void winGLCanvas1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = this.winGLCanvas1.Height - e.Y - 1;
            List<HitTarget> list = this.scene.Pick(x, y);
            //foreach (var item in list)
            //{
            //    var parent = item.renderer.Parent;
            //    if (parent != null)
            //    {
            //        var renderer = parent as IRenderable;
            //        if (renderer != null)
            //        {
            //            renderer.RenderingEnabled = !renderer.RenderingEnabled;
            //        }
            //    }
            //}

            if (list.Count == 0)
            {
                this.propGrid.SelectedObject = null;
            }
            else if (list.Count == 1)
            {
                this.propGrid.SelectedObject = list[0].renderer;
            }
            else
            {
                this.propGrid.SelectedObjects = (from item in list select item.renderer).ToArray();
            }

            this.lblState.Text = string.Format("{0} objects selected.", list.Count);
        }

        private void trvScene_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.propGrid.SelectedObject = e.Node.Tag;

            this.lblState.Text = string.Format("{0} objects selected.", 1);
        }
    }
}
