﻿using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vectrosity;

namespace RealAntennas.Network
{
    class RACommNetUI : CommNet.CommNetUI
    {
        public Color colorToTarget = XKCDColors.BananaYellow;
        public Color colorNormal = XKCDColors.BananaYellow;
        public Color color3dB = XKCDColors.VividPurple;
        public Color color10dB = XKCDColors.AquaMarine;

        VectorLine targetLine = null;
        VectorLine cone3Line = null;
        VectorLine cone10Line = null;

        protected virtual void CreateLine(string name, ref VectorLine l, List<Vector3> points)
        {
            if (l != null)
                VectorLine.Destroy(ref l);
            l = new VectorLine(name, points, this.lineWidth2D, Vectrosity.LineType.Discrete);
            l.rectTransform.gameObject.layer = 31;
            l.material = this.lineMaterial;
            l.texture = this.lineTexture;
            l.smoothColor = this.smoothColor;
            l.UpdateImmediate = true;
        }

        public void GatherAntennaCones(RACommNode node, ref List<Vector3> targetPoints, ref List<Vector3> cone3Points, ref List<Vector3> cone10Points)
        {
            if (node == null || node.RAAntennaList.Count == 0) return;
            foreach (RealAntenna ra in node.RAAntennaList)
            {
                if (ra.CanTarget && ra.Target != null)
                {
                    targetPoints.Add(node.position);
                    targetPoints.Add(ra.Target.GetTransform().position);

                    Vector3d perpToCamera = new Vector3d(1, 0, 0);
//                    Vector3d perpToCamera = Vector3d.Cross(node.position - VectorLine.camTransformPosition,
//                                                        ra.Target.GetTransform().position - VectorLine.camTransformPosition);
                    perpToCamera.Normalize();
                    Vector3 toTarget = ra.Target.GetTransform().position - node.position;
                    Vector3 leftbound3 = Vector3.RotateTowards(
                        toTarget,
                        perpToCamera,
                        Convert.ToSingle(ra.Beamwidth * Math.PI / 180),
                        0);
                    Vector3 rightbound3 = Vector3.RotateTowards(
                        toTarget,
                        perpToCamera,
                        -1 * Convert.ToSingle(ra.Beamwidth * Math.PI / 180),
                        0);
                    Vector3 leftbound10 = Vector3.RotateTowards(toTarget,perpToCamera,Convert.ToSingle(ra.Beamwidth/2 * Math.PI / 180),0);
                    Vector3 rightbound10 = Vector3.RotateTowards(toTarget,perpToCamera,-1 * Convert.ToSingle(ra.Beamwidth/2 * Math.PI / 180),0);

                    float coneScale = Convert.ToSingle(1e7);
                    leftbound3.Normalize();
                    leftbound10.Normalize();
                    rightbound3.Normalize();
                    rightbound10.Normalize();
                    leftbound3 *= coneScale;
                    leftbound10 *= coneScale;
                    rightbound3 *= coneScale;
                    rightbound10 *= coneScale;

                    cone3Points.Add(leftbound3 + node.position);
                    cone3Points.Add(node.position);
                    cone3Points.Add(rightbound3 + node.position);
                    cone3Points.Add(node.position);

                    cone10Points.Add(leftbound10 + node.position);
                    cone10Points.Add(node.position);
                    cone10Points.Add(rightbound10 + node.position);
                    cone10Points.Add(node.position);
                }
            }
        }


        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();
            if (CommNetNetwork.Instance == null) return;
            if (CommNetUI.Mode == CommNetUI.DisplayMode.None) return;
            List<Vector3> targetPoints = new List<Vector3>();
            List<Vector3> cone3Points = new List<Vector3>();
            List<Vector3> cone10Points = new List<Vector3>();

            // We COULD but do not need to rewrite how CommNetUI displays its normal link stuff.
            // Let's just get started by displaying antenna cones for now, for nodes the UI asks about.

            CommNetwork commNet = CommNetNetwork.Instance.CommNet;
            CommNetVessel commNetVessel = null;
            CommNode commNode = null;
            CommPath commPath = null;
            if (this.vessel != null && this.vessel.connection != null && this.vessel.connection.Comm.Net != null)
            {
                commNetVessel = this.vessel.connection;
                commNode = commNetVessel.Comm;
                commPath = commNetVessel.ControlPath;
            }
            // Big switch statement seeking cases of nothing to do.
            switch (CommNetUI.Mode)
            {
                case DisplayMode.FirstHop:
                    GatherAntennaCones(commNode as RACommNode, ref targetPoints, ref cone3Points, ref cone10Points);
                    break;
                case CommNetUI.DisplayMode.Path:
                    foreach (CommLink link in commPath)
                    {
                        GatherAntennaCones(link.start as RACommNode, ref targetPoints, ref cone3Points, ref cone10Points);
                    }
                    break;
                case CommNetUI.DisplayMode.Network:
                    for (int i=0;i<commNet.Count;i++)
                    {
                        CommNode node = commNet[i];
                        GatherAntennaCones(node as RACommNode, ref targetPoints, ref cone3Points, ref cone10Points);
                    }
                    break;
            }
            ScaledSpace.LocalToScaledSpace(targetPoints);
            CreateLine(ref targetLine, targetPoints);
            targetLine.name = "RACommNetUIVectorToTarget";
            targetLine.SetColor(colorToTarget);
            targetLine.active = true;

            ScaledSpace.LocalToScaledSpace(cone3Points);
            CreateLine(ref cone3Line, cone3Points);
            cone3Line.name = "RACommNetUIVectorCone3dB";
            cone3Line.SetColor(color3dB);
            cone3Line.active = true;

            ScaledSpace.LocalToScaledSpace(cone10Points);
            CreateLine(ref cone10Line, cone10Points);
            cone10Line.name = "RACommNetUIVectorCone10dB";
            cone10Line.SetColor(color10dB);
            cone10Line.active = true;
            float width = draw3dLines ? lineWidth3D : lineWidth2D;
            targetLine.SetWidth(width);
            cone3Line.SetWidth(width);
            cone10Line.SetWidth(width);
            if (this.draw3dLines)
            {
                targetLine.Draw3D();
                cone3Line.Draw3D();
                cone10Line.Draw3D();
            }
            else
            {
                targetLine.Draw();
                cone3Line.Draw();
                cone10Line.Draw();
            }
        }
    }
}