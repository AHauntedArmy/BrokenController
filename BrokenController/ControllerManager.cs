using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using GorillaLocomotion;

using System.Reflection;
using HarmonyLib; // for accesstools

namespace BrokenController
{
    internal class XRNodeController
    {
        public VRMap OfflineHand { get; private set; }
        public XRController Controller { get; private set; }

        public VRMap OnlineHand {
            get => onlineHand;
            set {
                if (value != null) {
                    onlineHand = value;
                    onlineHand.vrTargetNode = OfflineHand.vrTargetNode;
                    return;
                }

                onlineHand = value;
            }
        }

        private VRMap onlineHand = null;
        
        public bool DeviceValid {
            get {
                if (Controller == null) {
                    return false;
                }

                return Controller.inputDevice.isValid;
            }
        }

        private XRNodeController() { }
        public XRNodeController(VRMap hand, XRController xrController)
        {
            OfflineHand = hand;
            Controller = xrController;
        }

        public void SetXRNode(XRNode controllerNode)
        {
            OfflineHand.vrTargetNode = controllerNode;
            Controller.controllerNode = controllerNode;
            
            if (OnlineHand != null) {
                OnlineHand.vrTargetNode = controllerNode;
            }
        }
    }

    public class ControllerManager : MonoBehaviour
    {
        private const InputDeviceCharacteristics rightCharecteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
        private const InputDeviceCharacteristics leftCharecteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        XRNodeController rightHandRig = null;
        XRNodeController leftHandRig = null;

        private bool rightValid = false;
        private bool leftValid = false;

        private FieldInfo controllerInputDevice;

        void Awake()
        {
            var rightController = Player.Instance?.rightHandTransform.GetComponent<XRController>();
            var leftController = Player.Instance?.leftHandTransform.GetComponent<XRController>();

            if (rightController == null) {
                Debug.Log("BrokenController: right controller script not found");
                Object.Destroy(this);
                return;
            
            } else Debug.Log(rightController.controllerNode.ToString());

            if (leftController == null) {
                Debug.Log("BrokenController: left controller script not found");
                Object.Destroy(this);
                return;
            
            } else Debug.Log(leftController.controllerNode.ToString());

            controllerInputDevice = AccessTools.Field(typeof(XRController), "m_InputDevice");

            VRRig offlineRig = null;
            foreach (var rig in Resources.FindObjectsOfTypeAll<VRRig>()) {
                if (rig.isOfflineVRRig) {
                    offlineRig = rig;
                }
            }

            if (offlineRig == null) {
                Debug.Log("BrokenController: failed to find the players vrrig");
                GameObject.Destroy(this);
                return;
            }

            var rightHand = offlineRig.rightHand;
            var leftHand = offlineRig.leftHand;

            if (rightHand == null || leftHand == null) {
                Debug.Log("BrokenController: failed to find offline rig hands.");
                GameObject.Destroy(this);
                return;
            }

            rightHandRig = new XRNodeController(rightHand, rightController);
            leftHandRig = new XRNodeController(leftHand, leftController);
        }

        private void OnEnable()
        {
            rightValid = rightHandRig.DeviceValid;
            leftValid = leftHandRig.DeviceValid;

            Debug.Log("BrokenController: RightController isvalid: " + rightValid);
            Debug.Log("BrokenController: LeftController isvalid: " + leftValid);

            UpdateControllers();

            InputDevices.deviceConnected += DeviceConnected;
            InputDevices.deviceDisconnected += DeviceDisconnected;
        }

        private void OnDisable()
        {
            rightValid = false;
            leftValid = false;

            UpdateControllers();

            InputDevices.deviceConnected -= DeviceConnected;
            InputDevices.deviceDisconnected -= DeviceDisconnected;
        }

        private void DeviceConnected(InputDevice device)
        {
            if ((device.characteristics & rightCharecteristics) == rightCharecteristics) {
                rightValid = true;
            }

            if ((device.characteristics & leftCharecteristics) == leftCharecteristics) {
                leftValid = true;
            }

            UpdateControllers();
        }

        private void DeviceDisconnected(InputDevice device)
        {
            if ((device.characteristics & rightCharecteristics) == rightCharecteristics) {
                rightValid = false;
            }

            if ((device.characteristics & leftCharecteristics) == leftCharecteristics) {
                leftValid = false;
            }

            UpdateControllers();
        }

        private void UpdateControllers()
        {
            if (rightHandRig == null || leftHandRig == null) {
                return;
            }

            Debug.Log("BrokenController: Right controller connected: " + rightValid);
            Debug.Log("BrokenController: Left controller connected: " + leftValid);

            if (rightValid == leftValid) {
                Debug.Log("setting the controllers back to default");
                rightHandRig.SetXRNode(XRNode.RightHand);
                leftHandRig.SetXRNode(XRNode.LeftHand);

                controllerInputDevice.SetValue(rightHandRig.Controller, InputDevices.GetDeviceAtXRNode(XRNode.RightHand));
                controllerInputDevice.SetValue(leftHandRig.Controller, InputDevices.GetDeviceAtXRNode(XRNode.LeftHand));

                return;
            }

            if (rightValid) {
                Debug.Log("moving left hand to the right controller");
                leftHandRig.SetXRNode(XRNode.RightHand);
            }

            if (leftValid) {
                Debug.Log("moving right hand to the left controller");
                rightHandRig.SetXRNode(XRNode.LeftHand);
            }
        }

        public void AddOnlineRig(VRRig onlineRig)
        {
            if (onlineRig.photonView.IsMine) {
                rightHandRig.OnlineHand = onlineRig.rightHand;
                leftHandRig.OnlineHand = onlineRig.leftHand;
            }
        }

        public void RemoveOnlineRig(VRRig onlineRig)
        {
            if (onlineRig.photonView.IsMine) {
                rightHandRig.OnlineHand = null;
                leftHandRig.OnlineHand = null;
            }
        }
    }
}