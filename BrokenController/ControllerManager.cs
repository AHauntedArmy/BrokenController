using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using GorillaLocomotion;

using System.Reflection;
using HarmonyLib; // for accesstools

namespace BrokenController
{
    public class ControllerManager : MonoBehaviour
    {
        private const InputDeviceCharacteristics rightCharecteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
        private const InputDeviceCharacteristics leftCharecteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        private XRController rightController = null;
        private XRController leftController = null;

        private VRMap rightHandRig = null;
        private VRMap leftHandRig = null;
        private VRMap onlineRightHandRig = null;
        private VRMap onlineLeftHandRig = null;

        private bool rightValid = false;
        private bool leftValid = false;

        private FieldInfo controllerInputDevice;

        void Awake()
        {
            rightController = Player.Instance?.rightHandTransform.GetComponent<XRController>();
            leftController = Player.Instance?.leftHandTransform.GetComponent<XRController>();

            if (rightController == null) {
                Debug.Log("right controller script not found");
                Object.Destroy(this);
                return;
            
            } else Debug.Log(rightController.controllerNode.ToString());

            if (leftController == null) {
                Debug.Log("left controller script not found");
                Object.Destroy(this);
                return;
            
            } else Debug.Log(leftController.controllerNode.ToString());

            controllerInputDevice = AccessTools.Field(typeof(XRController), "m_InputDevice");

            GameObject rigObject = null;
            VRRig offlineRig = null;

            rigObject = GameObject.Find("Actual Gorilla");
            offlineRig = rigObject?.GetComponent<VRRig>();

            if (offlineRig == null) {
                GameObject.Destroy(this);
                return;
            }

            rightHandRig = offlineRig.rightHand;
            leftHandRig = offlineRig.leftHand;

            if (rightHandRig == null || leftHandRig == null) {
                GameObject.Destroy(this);
                return;
            }
        }

        private void OnEnable()
        {
            rightValid = rightController.inputDevice.isValid;
            leftValid = leftController.inputDevice.isValid;

            Debug.Log("RightController isvalid: " + rightValid);
            Debug.Log("LeftController isvalid: " + leftValid);

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
            if ((device.characteristics & rightCharecteristics) == rightCharecteristics) rightValid = true;
            if ((device.characteristics & leftCharecteristics) == leftCharecteristics) leftValid = true;

            UpdateControllers();
        }

        private void DeviceDisconnected(InputDevice device)
        { 
            if ((device.characteristics & rightCharecteristics) == rightCharecteristics) rightValid = false;
            if ((device.characteristics & leftCharecteristics) == leftCharecteristics) leftValid = false;

            UpdateControllers();

            bool leftisvalid = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).isValid;
            bool rightisvalid = InputDevices.GetDeviceAtXRNode(XRNode.RightHand).isValid;

            Debug.Log("leftisvalid: " + leftisvalid);
            Debug.Log("rightisvalid: " + rightisvalid);
        }

        private void UpdateControllers()
        {
            Debug.Log("Right controller connected: " + rightValid);
            Debug.Log("Left controller connected: " + leftValid);

            if (rightValid == leftValid) {
                Debug.Log("setting the controllers back to default");
                if (onlineRightHandRig != null) onlineRightHandRig.vrTargetNode = XRNode.RightHand;
                rightHandRig.vrTargetNode = XRNode.RightHand;
                rightController.controllerNode = XRNode.RightHand;

                if (onlineLeftHandRig != null) onlineLeftHandRig.vrTargetNode = XRNode.LeftHand;
                leftHandRig.vrTargetNode = XRNode.LeftHand;
                leftController.controllerNode = XRNode.LeftHand;

                controllerInputDevice.SetValue(rightController, InputDevices.GetDeviceAtXRNode(XRNode.RightHand));
                controllerInputDevice.SetValue(leftController, InputDevices.GetDeviceAtXRNode(XRNode.LeftHand));

                Debug.Log(rightController.inputDevice.name);
                Debug.Log(leftController.inputDevice.name);
                return;
            }

            if (rightValid) {
                Debug.Log("moving left hand to the right controller");
                if (onlineLeftHandRig != null) onlineLeftHandRig.vrTargetNode = XRNode.RightHand;
                leftHandRig.vrTargetNode = XRNode.RightHand;
                leftController.controllerNode = XRNode.RightHand;
            }

            if (leftValid) {
                Debug.Log("moving right hand to the left controller");
                if (onlineRightHandRig != null) onlineRightHandRig.vrTargetNode = XRNode.LeftHand;
                rightHandRig.vrTargetNode = XRNode.LeftHand;
                rightController.controllerNode = XRNode.LeftHand;
            }
        }

        public void AddOnlineRig(VRRig onlineRig)
        {
            if (onlineRig.photonView.IsMine) {
                onlineRightHandRig = onlineRig.rightHand;
                onlineLeftHandRig = onlineRig.leftHand;
            }
        }

        public void RemoveOnlineRig(VRRig onlineRig)
        {
            if (onlineRig.photonView.IsMine) {
                if (onlineRig.rightHand == onlineRightHandRig) onlineRightHandRig = null;
                if (onlineRig.leftHand == onlineLeftHandRig) onlineLeftHandRig = null;
            }
        }
    }
}