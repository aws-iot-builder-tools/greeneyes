# General troubleshooting

In this file we will discuss how to troubleshoot the problems we have encountered while preparing this blog series that
is not specific to a particular blog post.

## Vagrant can't create a new VM because another application is using ports 1441 and 1442

An error like this:

```
Vagrant cannot forward the specified ports on this VM, since they
would collide with some other application that is already listening
on these ports. The forwarded port to 1441 is already in use
on the host machine.

To fix this, modify your current project's Vagrantfile to use another
port. Example, where '1234' would be replaced by a unique host port:

  config.vm.network :forwarded_port, guest: 1441, host: 1234

Sometimes, Vagrant will attempt to auto-correct this for you. In this
case, Vagrant was unable to. This is usually because the guest machine
is in a state which doesn't allow modifying port forwarding. You could
try 'vagrant reload' (equivalent of running a halt followed by an up)
so vagrant can attempt to auto-correct this upon booting. Be warned
that any unsaved work might be lost.
```

Indicates that Vagrant thinks another VM is using these ports. If you are trying to run the VMs from multiple blog posts
at the same time you will need to run them one at a time, use different ports for each, or disable port forwarding in
the Vagrantfile.

If no other VMs are running, and no other applications are using the ports, then you can safely ignore this error and
try again. There is a period of time between when Vagrant shuts down a VM and when it can be used again. If you recently
shut down another VM using these ports you may need to wait a minute before you can use them again.

If you want to disable port forwarding open the Vagrantfile and comment out the lines that start like this:

```
config.vm.network "forwarded_port"
```

## AWS API calls fail with an `InvalidSignatureException`

An error like this:

```
Error while getting the component versions of 'greeneyes.BashCaptureLoop' in 'us-east-1' from the account '123123123123' during publish.
An error occurred (InvalidSignatureException) when calling the ListComponentVersions operation: Signature expired: 20220506T144634Z is now earlier than 20220506T145537Z (20220506T150037Z - 5 min.)
```

Indicates that the clock in the virtual machine is out of sync. This can happen if the system is suspended for a long
time.

To fix this you can either restart the virtual machine by running these commands on the host:

```
vagrant halt
vagrant up
```

Or you can update the clock on the virtual machine with `ntpdate` by running this command:

```
sudo ntpdate pool.ntp.org
```

This requires `ntpdate` to be installed on the virtual machine. If it isn't installed you can install it
with `sudo apt install -y ntpdate`.

## AWS API calls fail with a `nodename nor servname provided` error

An error like this:

```
DNS failure
✖ nodename nor servname provided, or not known (iot.us-east-1.amazonaws.com:443 (http://iot.us-east-1.amazonaws.com:443/))
```

Indicates that the virtual machine is not connected to the Internet, or that its DNS is not configured correctly.

Try to reconnect to your network and validate your DNS settings.

## vagrant up fails because "$HOME/.aws" is missing

The VM needs AWS credentials to be able to access the AWS API. You must have AWS credentials and config
files in `$HOME/.aws` to use the default Vagrantfile. This makes sure there are AWS credentials
available before going ahead with the VM provisioning. It reduces the chance of the VM failing later due
to missing credentials.

If your credentials are in a different location you can change the location in the Vagrantfile. Search
for `$HOME/.aws` and change the path to the path you'd like to use. The path must contain a `credentials`
file and a `config` file.

If you'd like to configure the AWS CLI manually inside of the instance you can comment out the line in
the Vagrantfile that shares the `$HOME/.aws` path. However, if you do this the instructions may fail
at a later point.

## vagrant up hangs after "default: SSH auth method: private key"

If `vagrant up` hangs here, open the VirtualBox GUI and see if it says the session is locked.

If the session is locked do the following:

- Press CTRL-C in the terminal that is running `vagrant up` to forcibly exit the boot process
- Run `vagrant destroy` to clean up the VM
- Delete the `.vagrant` folder with the `rm -rf .vagrant` command
- Run `vagrant up` again

The `.vagrant` directory can get into an inconsistent state if the VM isn't shut down properly. This
appears to fix it on the systems we've tested but it will lose any changes you've made to the VM.

## (Windows specific) Vagrant error about missing 'oscdimg'

If you receive an error like this when trying to start the VM:

```
==> default: Preparing user data for cloud-init...
The executable 'oscdimg' Vagrant is trying to run was not
found in the %PATH% variable. This is an error. Please verify
this software is installed and on the path.
```

Try restarting the VM with the `vagrant reload` command

Full error:

```
$ ./up.sh
==> vagrant: You have requested to enabled the experimental flag with the following features:
==> vagrant:
==> vagrant: Features:  cloud_init, disks
==> vagrant:
==> vagrant: Please use with caution, as some of the features may not be fully
==> vagrant: functional yet.
Bringing machine 'default' up with 'virtualbox' provider...
==> default: Checking if box 'ubuntu/focal64' version '20220324.0.0' is up to date...
==> default: Clearing any previously set forwarded ports...
==> default: Clearing any previously set network interfaces...
==> default: Preparing network interfaces based on configuration...
    default: Adapter 1: nat
==> default: Forwarding ports...
    default: 22 (guest) => 2222 (host) (adapter 1)
==> default: Preparing user data for cloud-init...
The executable 'oscdimg' Vagrant is trying to run was not
found in the %PATH% variable. This is an error. Please verify
this software is installed and on the path.
```

## USB 3.0 passthrough (xHCI) is not working

If you have enabled USB 3.0 passthrough and have whitelisted your device and you can not see
it in the VM then there are a few things you can check.

Open a shell in the VM and run the following command:

```
dmesg | grep -i xhci
```

The output should look similar to this:

```
[    0.852571] xhci_hcd 0000:00:0c.0: xHCI Host Controller
[    0.853432] xhci_hcd 0000:00:0c.0: new USB bus registered, assigned bus number 1
[    0.856690] xhci_hcd 0000:00:0c.0: hcc params 0x04000000 hci version 0x100 quirks 0x000000000000b930
[    0.861154] usb usb1: Product: xHCI Host Controller
[    0.862053] usb usb1: Manufacturer: Linux 5.4.0-107-generic xhci-hcd
[    0.865856] xhci_hcd 0000:00:0c.0: xHCI Host Controller
[    0.866884] xhci_hcd 0000:00:0c.0: new USB bus registered, assigned bus number 2
[    0.868200] xhci_hcd 0000:00:0c.0: Host supports USB 3.0 SuperSpeed
[    0.871937] usb usb2: Product: xHCI Host Controller
[    0.872715] usb usb2: Manufacturer: Linux 5.4.0-107-generic xhci-hcd
```

If there is no output then it means either USB 3.0 passthrough is not enabled or the host
system does not support USB 3.0 passthrough.

Try changing the USB 3.0 passthrough (xHCI) to USB 2.0 passthrough (EHCI) and see if the
capture test runs properly.

## (Windows specific) Unable to run capture-test.sh

If you are unable to run capture-test.sh it could be one of two reasons:

- You attempted to run the script directly like this: `./capture-test.sh` and got a permission denied error. Try again
  with `. ./capture-test.sh`. This is due to Windows permission issues.
- You attempted to run the script and received an error containing `\r`. This happens when Git for Windows is configured
  to use "Windows-style" checkouts. You will need to change the default checkout style to "Checkout as-is, commit
  Unix-style line changes".

## "info failed: ssh connection failed: 'Socket error: Connection reset by peer'"

If you see this message it is usually related to a temporary failure. This error usually comes
from Multipass and it should automatically try again. If it happens multiple times, Multipass
and/or the VirtualBox service may need to be restarted.

## "timed out waiting for initialization to complete"

This isn't necessarily a problem but it indicates that the cloud-init process is taking a long time to complete. The
timeout is hardcoded so this can happen on a machine with a slower Internet connection or a machine with a slower CPU.

[Multipass has an open issue related to this](https://github.com/canonical/multipass/issues/1039) and they are working
on customizing the timeout value.

If this happens during initialization on your system, and the resulting VM is not working you can try purging the VM and
then reinitializing it with the `--no-stop` flag like this:

```
./purge.sh
./init.sh --no-stop
```

You should see the message `Not stopping VM after initialzation` in the output to confirm that the VM is not being
stopped. Remember to stop the VM before trying to change any settings in the VirtualBox GUI.

## (Windows specific) Cannot execute scripts on the VM

When running VirtualBox on Windows we cannot run a script directly if it is in the mounted directory. This is because
the drive is mounted via SSHFS and SSHFS does not have a mechanism to translate the Linux executable flags to Windows
flags.

Normally we could run the `capture-test.sh` script like this:

```
./capture-test.sh
```

But this will not work. It will return a permission denied error.

Instead, run the script like this:

```
bash ./capture-test.sh
```

Other interpreted scripts (Python, NodeJS) will have the same issue. So always put the name of the interpreter (bash,
python3, node) as the first argument when running a script.

## Cannot connect to Multipass

If Multipass commands are failing and report an error message similar to this:

```
list failed: cannot connect to the multipass socket
Please ensure multipassd is running and 'localhost:50051' is accessible
```

Then `multipassd` may have crashed. While it is possible to restart it, it is recommended to restart the machine.

## Capturing an image from the camera hangs

If the camera is not capturing an image, it is likely that the camera is not connected to the VM using USB 2.x or
greater.

If you have selected USB 2.x in the VirtualBox GUI you can try stopping the VM, changing the USB controller to USB 3.x,
and restarting the VM.

## Multipass VM do not show up in the VirtualBox GUI

If the Multipass VM does not show up in the VirtualBox GUI it is usually either:

- The VirtualBox GUI is not running as an elevated user (root on Linux, MacOS or Administrator on Windows). Make sure
  the GUI is running as an elevated user by starting it with the `./gui.sh` script.
- The Multipass VM list is collapsed in the VirtualBox GUI. Check the list on the left side of the VirtualBox GUI see if
  there is an item titled `Multipass` under the `Tools` box. If there is, expand the list by clicking the arrow on the
  left side of it.

## VM hangs when starting it via a script

If the VM hangs when any of the scripts try to start it you can break out of the script (CTRL-C), and then either:

- Stop the VM and try to restart it
- Purge the VM and try to recreate it

Only purge the VM if you stopping it and restarting it fails.

## Unable to update VM configuration in VirtualBox GUI

Sometimes the VirtualBox GUI reports this error when trying to update the VM configuration:

```
The virtual machine that you are changing has been started. Only certain settings can be changed while a machine is running. All other changes will be lost if you close this window now.
```

If this happens:

- Make sure the VM is stopped in VirtualBox. If it is running, stop it and try again.
- If the VM is stopped, check to see if Multipass still thinks the VM is running with the `multipass list` command. If
  it shows up in the list as running, then Multipass and VirtualBox are out of sync. There are two options to fix this
  issue before trying again:
    - (Recommended) Reboot the host
    - (NOT recommended) Force the Multipass daemon to stop
        - Linux and MacOS `sudo killall multipassd`
        - TODO - VALIDATE THIS - Windows `taskkill /F /IM multipassd.exe`
- (Windows only) If you are still unable to use the VirtualBox GUI try closing the VirtualBox GUI, stopping and
  restarting the VirtualBox system service, and opening the VirtualBox GUI with the `gui.sh` script again

## Unable to fetch some archives

Sometimes `sudo apt-get update` reports this error:

```
E: Unable to fetch some archives, maybe run apt-get update or try with --fix-missing?
```

And sometimes it is accompanied by this error:

```
Hash Sum mismatch
```

From what we've seen this is usually due to a networking issue on the host. The best course of action is to stop the VM
with the `./stop.sh` script, reboot the host, and reconnected to the VM with the `./start.sh` script.

This kind of networking issue can be caused by:

- Hardware issues on the host
- Security software (like firewalls, VPNs) on the host terminating TLS connections prematurely
- Security hardware (like firewalls, proxies, IPS) on the network terminating TLS connections prematurely

# TODO

Hash mismatch is actually related to networking issues which I've seen in MacOS too

Worked after reboot

```
ubuntu@blog-post-001:~/001/guest$ curl -L https://github.com/docker/compose/releases/download/1.26.1/docker-compose-Linux-x86_64 > x
  % Total    % Received % Xferd  Average Speed   Time    Time     Time  Current
                                 Dload  Upload   Total   Spent    Left  Speed
100   664  100   664    0     0   9352      0 --:--:-- --:--:-- --:--:--  9352
 55 11.6M   55 6642k    0     0  15.0M      0 --:--:-- --:--:-- --:--:-- 15.0M
curl: (56) OpenSSL SSL_read: error:1408F10B:SSL routines:ssl3_get_record:wrong version number, errno 0
ubuntu@blog-post-001:~/001/guest$ curl -L https://github.com/docker/compose/releases/download/1.26.1/docker-compose-Linux-x86_64 > x
  % Total    % Received % Xferd  Average Speed   Time    Time     Time  Current
                                 Dload  Upload   Total   Spent    Left  Speed
100   664  100   664    0     0   9910      0 --:--:-- --:--:-- --:--:--  9910
 83 11.6M   83  9.8M    0     0  16.2M      0 --:--:-- --:--:-- --:--:-- 16.2M
curl: (56) OpenSSL SSL_read: error:1408F119:SSL routines:ssl3_get_record:decryption failed or bad record mac, errno 0
```

Strange, this hash sum mismatch error happened multiple times in a row today.

```
ubuntu@blog-post-001:~/001/guest$ sudo apt-get update && sudo apt-get install -y linux-generic
Get:1 http://security.ubuntu.com/ubuntu focal-security InRelease [114 kB]
Hit:2 http://archive.ubuntu.com/ubuntu focal InRelease
Get:3 http://archive.ubuntu.com/ubuntu focal-updates InRelease [114 kB]
Get:4 http://archive.ubuntu.com/ubuntu focal-backports InRelease [108 kB]
Get:5 http://archive.ubuntu.com/ubuntu focal/universe amd64 Packages [8628 kB]
Get:6 http://archive.ubuntu.com/ubuntu focal/universe Translation-en [5124 kB]
Get:7 http://security.ubuntu.com/ubuntu focal-security/main amd64 Packages [1375 kB]
Get:8 http://security.ubuntu.com/ubuntu focal-security/main Translation-en [238 kB]
Get:9 http://security.ubuntu.com/ubuntu focal-security/main amd64 c-n-f Metadata [9876 B]
Get:10 http://security.ubuntu.com/ubuntu focal-security/restricted amd64 Packages [850 kB]
Get:11 http://security.ubuntu.com/ubuntu focal-security/restricted Translation-en [121 kB]
Get:12 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Err:12 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages
  Hash Sum mismatch
  Hashes of expected file:
   - Filesize:694908 [weak]
   - SHA256:c9c34c320378457664263f7fca1f21f1fb83d9226b50a2dc1a74e10a71d98e25
   - SHA1:2cd71ce9a3d6eb309714fb34e148154df6371c3b [weak]
   - MD5Sum:d579b8727d3721ec4acdf7ee614639bb [weak]
  Hashes of received file:
   - SHA256:f937545cf30a0d44f9f0917e38326c7e74c3f3a28dd250d9bdac11bee0f096c0
   - SHA1:4bb1b180fa6d1910140aab28daf22a63a3e3ce6a [weak]
   - MD5Sum:b8156cb4395205f04c0155b6b4a69781 [weak]
   - Filesize:694908 [weak]
  Last modification reported: Thu, 31 Mar 2022 13:49:29 +0000
  Release file created at: Thu, 31 Mar 2022 17:48:15 +0000
Get:13 http://security.ubuntu.com/ubuntu focal-security/universe Translation-en [122 kB]
Get:14 http://security.ubuntu.com/ubuntu focal-security/universe amd64 c-n-f Metadata [14.1 kB]
Get:15 http://security.ubuntu.com/ubuntu focal-security/multiverse amd64 Packages [20.7 kB]
Get:16 http://security.ubuntu.com/ubuntu focal-security/multiverse Translation-en [5196 B]
Get:17 http://security.ubuntu.com/ubuntu focal-security/multiverse amd64 c-n-f Metadata [500 B]
Get:18 http://archive.ubuntu.com/ubuntu focal/universe amd64 c-n-f Metadata [265 kB]
Get:19 http://archive.ubuntu.com/ubuntu focal/multiverse amd64 Packages [144 kB]
Get:20 http://archive.ubuntu.com/ubuntu focal/multiverse Translation-en [104 kB]
Get:21 http://archive.ubuntu.com/ubuntu focal/multiverse amd64 c-n-f Metadata [9136 B]
Get:22 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 Packages [1703 kB]
Get:23 http://archive.ubuntu.com/ubuntu focal-updates/main Translation-en [319 kB]
Get:24 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 c-n-f Metadata [14.8 kB]
Get:25 http://archive.ubuntu.com/ubuntu focal-updates/restricted amd64 Packages [910 kB]
Get:26 http://archive.ubuntu.com/ubuntu focal-updates/restricted Translation-en [130 kB]
Get:27 http://archive.ubuntu.com/ubuntu focal-updates/universe amd64 Packages [915 kB]
Get:28 http://archive.ubuntu.com/ubuntu focal-updates/universe Translation-en [204 kB]
Get:29 http://archive.ubuntu.com/ubuntu focal-updates/universe amd64 c-n-f Metadata [20.4 kB]
Get:30 http://archive.ubuntu.com/ubuntu focal-updates/multiverse amd64 Packages [24.4 kB]
Get:31 http://archive.ubuntu.com/ubuntu focal-updates/multiverse Translation-en [7336 B]
Get:32 http://archive.ubuntu.com/ubuntu focal-updates/multiverse amd64 c-n-f Metadata [592 B]
Get:33 http://archive.ubuntu.com/ubuntu focal-backports/main amd64 Packages [42.2 kB]
Get:34 http://archive.ubuntu.com/ubuntu focal-backports/main Translation-en [10.1 kB]
Get:35 http://archive.ubuntu.com/ubuntu focal-backports/main amd64 c-n-f Metadata [864 B]
Get:36 http://archive.ubuntu.com/ubuntu focal-backports/restricted amd64 c-n-f Metadata [116 B]
Get:37 http://archive.ubuntu.com/ubuntu focal-backports/universe amd64 Packages [22.7 kB]
Get:38 http://archive.ubuntu.com/ubuntu focal-backports/universe Translation-en [15.5 kB]
Get:39 http://archive.ubuntu.com/ubuntu focal-backports/universe amd64 c-n-f Metadata [808 B]
Get:40 http://archive.ubuntu.com/ubuntu focal-backports/multiverse amd64 c-n-f Metadata [116 B]
Fetched 22.4 MB in 4s (6213 kB/s)
Reading package lists... Done
E: Failed to fetch http://security.ubuntu.com/ubuntu/dists/focal-security/universe/binary-amd64/by-hash/SHA256/c9c34c320378457664263f7fca1f21f1fb83d9226b50a2dc1a74e10a71d98e25  Hash Sum mismatch
   Hashes of expected file:
    - Filesize:694908 [weak]
    - SHA256:c9c34c320378457664263f7fca1f21f1fb83d9226b50a2dc1a74e10a71d98e25
    - SHA1:2cd71ce9a3d6eb309714fb34e148154df6371c3b [weak]
    - MD5Sum:d579b8727d3721ec4acdf7ee614639bb [weak]
   Hashes of received file:
    - SHA256:f937545cf30a0d44f9f0917e38326c7e74c3f3a28dd250d9bdac11bee0f096c0
    - SHA1:4bb1b180fa6d1910140aab28daf22a63a3e3ce6a [weak]
    - MD5Sum:b8156cb4395205f04c0155b6b4a69781 [weak]
    - Filesize:694908 [weak]
   Last modification reported: Thu, 31 Mar 2022 13:49:29 +0000
   Release file created at: Thu, 31 Mar 2022 17:48:15 +0000
E: Some index files failed to download. They have been ignored, or old ones used instead.
ubuntu@blog-post-001:~/001/guest$ sudo apt-get update && sudo apt-get install -y linux-generic
Hit:1 http://archive.ubuntu.com/ubuntu focal InRelease
Get:2 http://security.ubuntu.com/ubuntu focal-security InRelease [114 kB]
Hit:3 http://archive.ubuntu.com/ubuntu focal-updates InRelease
Hit:4 http://archive.ubuntu.com/ubuntu focal-backports InRelease
Get:5 http://security.ubuntu.com/ubuntu focal-security/main amd64 Packages [1375 kB]
Get:6 http://security.ubuntu.com/ubuntu focal-security/main Translation-en [238 kB]
Get:7 http://security.ubuntu.com/ubuntu focal-security/main amd64 c-n-f Metadata [9876 B]
Get:8 http://security.ubuntu.com/ubuntu focal-security/restricted amd64 Packages [850 kB]
Get:9 http://security.ubuntu.com/ubuntu focal-security/restricted Translation-en [121 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Get:10 http://security.ubuntu.com/ubuntu focal-security/universe amd64 Packages [695 kB]
Fetched 809 kB in 1s (577 kB/s)
Reading package lists... Done
Building dependency tree
Reading state information... Done
19 packages can be upgraded. Run 'apt list --upgradable' to see them.
Reading package lists... Done
Building dependency tree
Reading state information... Done
The following additional packages will be installed:
  amd64-microcode crda intel-microcode iucode-tool iw libdbus-glib-1-2 libevdev2 libimobiledevice6 libnl-3-200 libnl-genl-3-200
  libplist3 libupower-glib3 libusbmuxd6 linux-firmware linux-headers-5.4.0-107 linux-headers-5.4.0-107-generic
  linux-headers-generic linux-headers-virtual linux-image-5.4.0-107-generic linux-image-generic linux-image-virtual
  linux-modules-5.4.0-107-generic linux-modules-extra-5.4.0-107-generic linux-virtual thermald upower usbmuxd wireless-regdb
Suggested packages:
  libusbmuxd-tools fdutils linux-doc | linux-source-5.4.0 linux-tools
The following NEW packages will be installed:
  amd64-microcode crda intel-microcode iucode-tool iw libdbus-glib-1-2 libevdev2 libimobiledevice6 libnl-3-200 libnl-genl-3-200
  libplist3 libupower-glib3 libusbmuxd6 linux-firmware linux-generic linux-headers-5.4.0-107 linux-headers-5.4.0-107-generic
  linux-image-5.4.0-107-generic linux-image-generic linux-modules-5.4.0-107-generic linux-modules-extra-5.4.0-107-generic
  thermald upower usbmuxd wireless-regdb
The following packages will be upgraded:
  linux-headers-generic linux-headers-virtual linux-image-virtual linux-virtual
4 upgraded, 25 newly installed, 0 to remove and 15 not upgraded.
Need to get 207 MB of archives.
After this operation, 1048 MB of additional disk space will be used.
Get:1 http://archive.ubuntu.com/ubuntu focal/main amd64 libnl-3-200 amd64 3.4.0-1 [53.9 kB]
Get:2 http://archive.ubuntu.com/ubuntu focal/main amd64 libnl-genl-3-200 amd64 3.4.0-1 [11.1 kB]
Get:3 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 wireless-regdb all 2021.08.28-0ubuntu1~20.04.1 [10.0 kB]
Get:4 http://archive.ubuntu.com/ubuntu focal/main amd64 iw amd64 5.4-1 [94.0 kB]
Get:5 http://archive.ubuntu.com/ubuntu focal/main amd64 crda amd64 3.18-1build1 [63.5 kB]
Get:6 http://archive.ubuntu.com/ubuntu focal/main amd64 iucode-tool amd64 2.3.1-1 [45.6 kB]
Get:7 http://archive.ubuntu.com/ubuntu focal/main amd64 libdbus-glib-1-2 amd64 0.110-5fakssync1 [59.1 kB]
Get:8 http://archive.ubuntu.com/ubuntu focal/main amd64 libplist3 amd64 2.1.0-4build2 [31.6 kB]
Get:9 http://archive.ubuntu.com/ubuntu focal/main amd64 libusbmuxd6 amd64 2.0.1-2 [19.1 kB]
Get:10 http://archive.ubuntu.com/ubuntu focal/main amd64 libimobiledevice6 amd64 1.2.1~git20191129.9f79242-1build1 [65.2 kB]
Get:11 http://archive.ubuntu.com/ubuntu focal/main amd64 libupower-glib3 amd64 0.99.11-1build2 [43.2 kB]
Get:12 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-firmware all 1.187.29 [125 MB]
Err:12 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-firmware all 1.187.29
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:3a1292d4494e7c2b568aa1190db9e05cb8c76a6151c0e1725e7ab345f7679b1b0a18f230b5f738aa681e3bd2e3ac952835c5e598cedaa41800b1a4c5ddf5ea60
   - SHA256:22697f12ade7e6d6a2dd9ac956f594a3f5e2697ada3a29916fee465cc83a34a1
   - SHA1:d5b38ddd66efbb2971ea33efe7140942d728abac [weak]
   - MD5Sum:0bb7dcd5f36bb88e74d056a1fb4292eb [weak]
   - Filesize:124715796 [weak]
  Hashes of received file:
   - SHA512:6e70e10d4cc39461587e6e6896de7533ce8129ee3776ca242ff4bce496fcb8440de5203da9fbaf87918516b4014b84aa14cdb18353033236d48f763780fe945d
   - SHA256:649283aa990ccd1118bd5eb44fcb9aaff476743e431cadfdc56ab9bfa1d46267
   - SHA1:d609d042204199362c6d330a599db056ce53b9d3 [weak]
   - MD5Sum:75f0c620bcb38b5516bae65c9bac0063 [weak]
   - Filesize:124715796 [weak]
  Last modification reported: Thu, 10 Mar 2022 10:33:57 +0000
Get:13 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-5.4.0-107-generic amd64 5.4.0-107.121 [14.8 MB]
Err:13 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-5.4.0-107-generic amd64 5.4.0-107.121
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:5d0ada8fd9b674ba175dc7d5a9faea870618c556ca21016d91c651802d3db6fee862433809bbf3f89621f03971f3a6c5402b6024e2c290522c2b592eeaa6bde9
   - SHA256:4844061ee7b5e097f7fe393147370d72a68056197361544f46d497717a230fbf
   - SHA1:6500a44ae5f9681bab147a98b2c2443df576aca2 [weak]
   - MD5Sum:47f2ff03fe1e7f1d56a7e835d832b982 [weak]
   - Filesize:14835136 [weak]
  Hashes of received file:
   - SHA512:eaa85edffaa85589dc63b0180d5aeadfcbbb1c5a46359022933ce0cd30b84ab8c4c501ed8580c4b5d1d1c4e1700400b4a1394763f4512c99e3db2e84c29714b3
   - SHA256:8f2b8b0097052e738101e49cb9130d88fc14aef6197a056cc15aec191edacd5d
   - SHA1:ba5bd1ae1a59e4c21f82f765d6f1adb6e88e279b [weak]
   - MD5Sum:c8b2904b0716f90761a144fda2585527 [weak]
   - Filesize:14835136 [weak]
  Last modification reported: Fri, 25 Mar 2022 10:48:07 +0000
Get:14 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-image-5.4.0-107-generic amd64 5.4.0-107.121 [10.5 MB]
Get:15 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-extra-5.4.0-107-generic amd64 5.4.0-107.121 [39.4 MB]
Err:15 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-extra-5.4.0-107-generic amd64 5.4.0-107.121
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:aed3da0b2d4385ede8ea405568a3533ba3ba01c0a719e9a64852c08d724a55ead6bf8d7993172a6d183c08cddc8a5cd620777d2e10b83be21cc9f180f2124fe9
   - SHA256:14d279434f416f3291a6da957c46d96d49500208bfe89f835252a8ac8e1bcaef
   - SHA1:c93dd293bbc17923d1b18784be24f9460bf8694b [weak]
   - MD5Sum:d43262205cf60ab0e262d42d9e3adf52 [weak]
   - Filesize:39395132 [weak]
  Hashes of received file:
   - SHA512:79ef3a54c97644aca5f75a7d7d9a72a18a027e2b6494d37ab152f75715ecba18350bfe6a13916030057b1f5a1aac974178ce0dbc519afcb25849925ec4c359e4
   - SHA256:2116dfbfb6611a0876dbeca07e26fd0c51ab18fca17d7fb175fe5a1d83715d87
   - SHA1:e9264f6e651ef71663a4a245b64ccb7e2cfe8aac [weak]
   - MD5Sum:9865a2e682d86f57959431d97c7b90ce [weak]
   - Filesize:39395132 [weak]
  Last modification reported: Fri, 25 Mar 2022 10:47:55 +0000
Get:16 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 intel-microcode amd64 3.20210608.0ubuntu0.20.04.1 [3809 kB]
Err:16 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 intel-microcode amd64 3.20210608.0ubuntu0.20.04.1
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:9c2a60706145c5734fb746ec786b686e51984cea6658c6ce63704e51c33ef7ec551d9f5576616931c0c0561cc883c69402cddea8ef8589203381869f60c8b907
   - SHA256:77862171122b75f85480e6b799cf84c8a0242e453605215e303936c695f0d774
   - SHA1:660a5d9c6d1cdf714a7ce207dd1705ad658db814 [weak]
   - MD5Sum:46961d349c8ae9ba65afc1d39bf8b2b3 [weak]
   - Filesize:3808820 [weak]
  Hashes of received file:
   - SHA512:a6c8b1fc755bf54a1df08a84fba3116e4a0ee5f2e959e87141dca08711c7a73953a75a09b9bfa0fcd7a3236b06306cf80a5efb55ea30f147c92b547ff10beee8
   - SHA256:c3253667b3bba3e91741b340fafb53176471b2117c9d65e415c0d19afd11f5f8
   - SHA1:a95f2ac0c3d8c59f9686df33a2505efb30a668dc [weak]
   - MD5Sum:888dfda8258db4cf79bcc99f895d90ff [weak]
   - Filesize:3808820 [weak]
  Last modification reported: Wed, 09 Jun 2021 04:23:37 +0000
Get:17 http://archive.ubuntu.com/ubuntu focal/main amd64 amd64-microcode amd64 3.20191218.1ubuntu1 [31.8 kB]
Get:18 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-image-generic amd64 5.4.0.107.111 [2520 B]
Get:19 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-virtual amd64 5.4.0.107.111 [1876 B]
Get:20 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-image-virtual amd64 5.4.0.107.111 [2492 B]
Get:21 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-headers-virtual amd64 5.4.0.107.111 [1840 B]
Get:22 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-headers-5.4.0-107 all 5.4.0-107.121 [11.0 MB]
Get:23 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-headers-5.4.0-107-generic amd64 5.4.0-107.121 [1395 kB]
Get:24 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-headers-generic amd64 5.4.0.107.111 [2388 B]
Get:25 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-generic amd64 5.4.0.107.111 [1900 B]
Get:26 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libevdev2 amd64 1.9.0+dfsg-1ubuntu0.1 [31.6 kB]
Get:27 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 thermald amd64 1.9.1-1ubuntu0.6 [233 kB]
Get:28 http://archive.ubuntu.com/ubuntu focal/main amd64 upower amd64 0.99.11-1build2 [104 kB]
Get:29 http://archive.ubuntu.com/ubuntu focal/main amd64 usbmuxd amd64 1.1.1~git20191130.9af2b12-1 [38.4 kB]
Fetched 207 MB in 5s (40.2 MB/s)
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/l/linux-firmware/linux-firmware_1.187.29_all.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:3a1292d4494e7c2b568aa1190db9e05cb8c76a6151c0e1725e7ab345f7679b1b0a18f230b5f738aa681e3bd2e3ac952835c5e598cedaa41800b1a4c5ddf5ea60
    - SHA256:22697f12ade7e6d6a2dd9ac956f594a3f5e2697ada3a29916fee465cc83a34a1
    - SHA1:d5b38ddd66efbb2971ea33efe7140942d728abac [weak]
    - MD5Sum:0bb7dcd5f36bb88e74d056a1fb4292eb [weak]
    - Filesize:124715796 [weak]
   Hashes of received file:
    - SHA512:6e70e10d4cc39461587e6e6896de7533ce8129ee3776ca242ff4bce496fcb8440de5203da9fbaf87918516b4014b84aa14cdb18353033236d48f763780fe945d
    - SHA256:649283aa990ccd1118bd5eb44fcb9aaff476743e431cadfdc56ab9bfa1d46267
    - SHA1:d609d042204199362c6d330a599db056ce53b9d3 [weak]
    - MD5Sum:75f0c620bcb38b5516bae65c9bac0063 [weak]
    - Filesize:124715796 [weak]
   Last modification reported: Thu, 10 Mar 2022 10:33:57 +0000
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/l/linux/linux-modules-5.4.0-107-generic_5.4.0-107.121_amd64.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:5d0ada8fd9b674ba175dc7d5a9faea870618c556ca21016d91c651802d3db6fee862433809bbf3f89621f03971f3a6c5402b6024e2c290522c2b592eeaa6bde9
    - SHA256:4844061ee7b5e097f7fe393147370d72a68056197361544f46d497717a230fbf
    - SHA1:6500a44ae5f9681bab147a98b2c2443df576aca2 [weak]
    - MD5Sum:47f2ff03fe1e7f1d56a7e835d832b982 [weak]
    - Filesize:14835136 [weak]
   Hashes of received file:
    - SHA512:eaa85edffaa85589dc63b0180d5aeadfcbbb1c5a46359022933ce0cd30b84ab8c4c501ed8580c4b5d1d1c4e1700400b4a1394763f4512c99e3db2e84c29714b3
    - SHA256:8f2b8b0097052e738101e49cb9130d88fc14aef6197a056cc15aec191edacd5d
    - SHA1:ba5bd1ae1a59e4c21f82f765d6f1adb6e88e279b [weak]
    - MD5Sum:c8b2904b0716f90761a144fda2585527 [weak]
    - Filesize:14835136 [weak]
   Last modification reported: Fri, 25 Mar 2022 10:48:07 +0000
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/l/linux/linux-modules-extra-5.4.0-107-generic_5.4.0-107.121_amd64.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:aed3da0b2d4385ede8ea405568a3533ba3ba01c0a719e9a64852c08d724a55ead6bf8d7993172a6d183c08cddc8a5cd620777d2e10b83be21cc9f180f2124fe9
    - SHA256:14d279434f416f3291a6da957c46d96d49500208bfe89f835252a8ac8e1bcaef
    - SHA1:c93dd293bbc17923d1b18784be24f9460bf8694b [weak]
    - MD5Sum:d43262205cf60ab0e262d42d9e3adf52 [weak]
    - Filesize:39395132 [weak]
   Hashes of received file:
    - SHA512:79ef3a54c97644aca5f75a7d7d9a72a18a027e2b6494d37ab152f75715ecba18350bfe6a13916030057b1f5a1aac974178ce0dbc519afcb25849925ec4c359e4
    - SHA256:2116dfbfb6611a0876dbeca07e26fd0c51ab18fca17d7fb175fe5a1d83715d87
    - SHA1:e9264f6e651ef71663a4a245b64ccb7e2cfe8aac [weak]
    - MD5Sum:9865a2e682d86f57959431d97c7b90ce [weak]
    - Filesize:39395132 [weak]
   Last modification reported: Fri, 25 Mar 2022 10:47:55 +0000
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/i/intel-microcode/intel-microcode_3.20210608.0ubuntu0.20.04.1_amd64.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:9c2a60706145c5734fb746ec786b686e51984cea6658c6ce63704e51c33ef7ec551d9f5576616931c0c0561cc883c69402cddea8ef8589203381869f60c8b907
    - SHA256:77862171122b75f85480e6b799cf84c8a0242e453605215e303936c695f0d774
    - SHA1:660a5d9c6d1cdf714a7ce207dd1705ad658db814 [weak]
    - MD5Sum:46961d349c8ae9ba65afc1d39bf8b2b3 [weak]
    - Filesize:3808820 [weak]
   Hashes of received file:
    - SHA512:a6c8b1fc755bf54a1df08a84fba3116e4a0ee5f2e959e87141dca08711c7a73953a75a09b9bfa0fcd7a3236b06306cf80a5efb55ea30f147c92b547ff10beee8
    - SHA256:c3253667b3bba3e91741b340fafb53176471b2117c9d65e415c0d19afd11f5f8
    - SHA1:a95f2ac0c3d8c59f9686df33a2505efb30a668dc [weak]
    - MD5Sum:888dfda8258db4cf79bcc99f895d90ff [weak]
    - Filesize:3808820 [weak]
   Last modification reported: Wed, 09 Jun 2021 04:23:37 +0000
E: Unable to fetch some archives, maybe run apt-get update or try with --fix-missing?
ubuntu@blog-post-001:~/001/guest$ sudo apt-get update && sudo apt-get install -y linux-generic
Hit:1 http://archive.ubuntu.com/ubuntu focal InRelease
Hit:2 http://archive.ubuntu.com/ubuntu focal-updates InRelease
Hit:3 http://archive.ubuntu.com/ubuntu focal-backports InRelease
Get:4 http://security.ubuntu.com/ubuntu focal-security InRelease [114 kB]
Fetched 114 kB in 1s (223 kB/s)
Reading package lists... Done
Building dependency tree
Reading state information... Done
19 packages can be upgraded. Run 'apt list --upgradable' to see them.
Reading package lists... Done
Building dependency tree
Reading state information... Done
The following additional packages will be installed:
  amd64-microcode crda intel-microcode iucode-tool iw libdbus-glib-1-2 libevdev2 libimobiledevice6 libnl-3-200 libnl-genl-3-200
  libplist3 libupower-glib3 libusbmuxd6 linux-firmware linux-headers-5.4.0-107 linux-headers-5.4.0-107-generic
  linux-headers-generic linux-headers-virtual linux-image-5.4.0-107-generic linux-image-generic linux-image-virtual
  linux-modules-5.4.0-107-generic linux-modules-extra-5.4.0-107-generic linux-virtual thermald upower usbmuxd wireless-regdb
Suggested packages:
  libusbmuxd-tools fdutils linux-doc | linux-source-5.4.0 linux-tools
The following NEW packages will be installed:
  amd64-microcode crda intel-microcode iucode-tool iw libdbus-glib-1-2 libevdev2 libimobiledevice6 libnl-3-200 libnl-genl-3-200
  libplist3 libupower-glib3 libusbmuxd6 linux-firmware linux-generic linux-headers-5.4.0-107 linux-headers-5.4.0-107-generic
  linux-image-5.4.0-107-generic linux-image-generic linux-modules-5.4.0-107-generic linux-modules-extra-5.4.0-107-generic
  thermald upower usbmuxd wireless-regdb
The following packages will be upgraded:
  linux-headers-generic linux-headers-virtual linux-image-virtual linux-virtual
4 upgraded, 25 newly installed, 0 to remove and 15 not upgraded.
Need to get 183 MB/207 MB of archives.
After this operation, 1048 MB of additional disk space will be used.
Get:1 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-firmware all 1.187.29 [125 MB]
Err:1 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-firmware all 1.187.29
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:3a1292d4494e7c2b568aa1190db9e05cb8c76a6151c0e1725e7ab345f7679b1b0a18f230b5f738aa681e3bd2e3ac952835c5e598cedaa41800b1a4c5ddf5ea60
   - SHA256:22697f12ade7e6d6a2dd9ac956f594a3f5e2697ada3a29916fee465cc83a34a1
   - SHA1:d5b38ddd66efbb2971ea33efe7140942d728abac [weak]
   - MD5Sum:0bb7dcd5f36bb88e74d056a1fb4292eb [weak]
   - Filesize:124715796 [weak]
  Hashes of received file:
   - SHA512:2a7371017ba3192e752a1ee9c30092a0978a3457ffed0d98b356613abc89eb2c0cf41b980aa8687aaed199c3da5d89ce9027db5d7f80575dffa7ad3fbffa4a0c
   - SHA256:7c243a38992aa5fa3adcf3dc1f9ecadab7c6b0b327a61525e0853ac04eb16826
   - SHA1:a0a6c8f413939004342c4a51d9f87cacbfe84051 [weak]
   - MD5Sum:b49a727fe6cba3f02a4170552c3d0a4f [weak]
   - Filesize:124715796 [weak]
  Last modification reported: Thu, 10 Mar 2022 10:33:57 +0000
Get:2 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-5.4.0-107-generic amd64 5.4.0-107.121 [14.8 MB]
Get:3 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-extra-5.4.0-107-generic amd64 5.4.0-107.121 [39.4 MB]
Err:3 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 linux-modules-extra-5.4.0-107-generic amd64 5.4.0-107.121
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:aed3da0b2d4385ede8ea405568a3533ba3ba01c0a719e9a64852c08d724a55ead6bf8d7993172a6d183c08cddc8a5cd620777d2e10b83be21cc9f180f2124fe9
   - SHA256:14d279434f416f3291a6da957c46d96d49500208bfe89f835252a8ac8e1bcaef
   - SHA1:c93dd293bbc17923d1b18784be24f9460bf8694b [weak]
   - MD5Sum:d43262205cf60ab0e262d42d9e3adf52 [weak]
   - Filesize:39395132 [weak]
  Hashes of received file:
   - SHA512:4d0873096ce6b08c67c591e8bc78bf06f1f736f8a7923e51aa8d68a009315e19f3a317364b8ce4839db8c7aecd581cf044d0caa7dd35e8ba013ca2d42bb7ad98
   - SHA256:1c4d515cf4d9f5c1007c5723797348fc929d1451def0defdd44d016453e5cec3
   - SHA1:bcf209eae6fd2e161595577bd207fe6be46ff25d [weak]
   - MD5Sum:d0208a35070f39ab8c739890f9877766 [weak]
   - Filesize:39395132 [weak]
  Last modification reported: Fri, 25 Mar 2022 10:47:55 +0000
Get:4 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 intel-microcode amd64 3.20210608.0ubuntu0.20.04.1 [3809 kB]
Fetched 183 MB in 8s (23.7 MB/s)
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/l/linux-firmware/linux-firmware_1.187.29_all.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:3a1292d4494e7c2b568aa1190db9e05cb8c76a6151c0e1725e7ab345f7679b1b0a18f230b5f738aa681e3bd2e3ac952835c5e598cedaa41800b1a4c5ddf5ea60
    - SHA256:22697f12ade7e6d6a2dd9ac956f594a3f5e2697ada3a29916fee465cc83a34a1
    - SHA1:d5b38ddd66efbb2971ea33efe7140942d728abac [weak]
    - MD5Sum:0bb7dcd5f36bb88e74d056a1fb4292eb [weak]
    - Filesize:124715796 [weak]
   Hashes of received file:
    - SHA512:2a7371017ba3192e752a1ee9c30092a0978a3457ffed0d98b356613abc89eb2c0cf41b980aa8687aaed199c3da5d89ce9027db5d7f80575dffa7ad3fbffa4a0c
    - SHA256:7c243a38992aa5fa3adcf3dc1f9ecadab7c6b0b327a61525e0853ac04eb16826
    - SHA1:a0a6c8f413939004342c4a51d9f87cacbfe84051 [weak]
    - MD5Sum:b49a727fe6cba3f02a4170552c3d0a4f [weak]
    - Filesize:124715796 [weak]
   Last modification reported: Thu, 10 Mar 2022 10:33:57 +0000
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/l/linux/linux-modules-extra-5.4.0-107-generic_5.4.0-107.121_amd64.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:aed3da0b2d4385ede8ea405568a3533ba3ba01c0a719e9a64852c08d724a55ead6bf8d7993172a6d183c08cddc8a5cd620777d2e10b83be21cc9f180f2124fe9
    - SHA256:14d279434f416f3291a6da957c46d96d49500208bfe89f835252a8ac8e1bcaef
    - SHA1:c93dd293bbc17923d1b18784be24f9460bf8694b [weak]
    - MD5Sum:d43262205cf60ab0e262d42d9e3adf52 [weak]
    - Filesize:39395132 [weak]
   Hashes of received file:
    - SHA512:4d0873096ce6b08c67c591e8bc78bf06f1f736f8a7923e51aa8d68a009315e19f3a317364b8ce4839db8c7aecd581cf044d0caa7dd35e8ba013ca2d42bb7ad98
    - SHA256:1c4d515cf4d9f5c1007c5723797348fc929d1451def0defdd44d016453e5cec3
    - SHA1:bcf209eae6fd2e161595577bd207fe6be46ff25d [weak]
    - MD5Sum:d0208a35070f39ab8c739890f9877766 [weak]
    - Filesize:39395132 [weak]
   Last modification reported: Fri, 25 Mar 2022 10:47:55 +0000
E: Unable to fetch some archives, maybe run apt-get update or try with --fix-missing?
ubuntu@blog-post-001:~/001/guest$ sudo apt upgrade
Reading package lists... Done
Building dependency tree
Reading state information... Done
Calculating upgrade... Done
The following NEW packages will be installed:
  linux-headers-5.4.0-107 linux-headers-5.4.0-107-generic linux-image-5.4.0-107-generic linux-modules-5.4.0-107-generic
The following packages will be upgraded:
  cloud-init landscape-common libnetplan0 libpython3.8 libpython3.8-minimal libpython3.8-stdlib linux-headers-generic
  linux-headers-virtual linux-image-virtual linux-virtual netplan.io open-vm-tools python3-twisted python3-twisted-bin python3.8
  python3.8-minimal rsync tzdata zlib1g
19 upgraded, 4 newly installed, 0 to remove and 0 not upgraded.
14 standard security updates
Need to get 10.3 MB/48.0 MB of archives.
After this operation, 179 MB of additional disk space will be used.
Do you want to continue? [Y/n] y
Get:1 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 zlib1g amd64 1:1.2.11.dfsg-2ubuntu1.3 [53.8 kB]
Get:2 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libpython3.8 amd64 3.8.10-0ubuntu1~20.04.4 [1625 kB]
Err:2 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libpython3.8 amd64 3.8.10-0ubuntu1~20.04.4
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:beabfa893ddbff17975f605fdd7caad35ea07b39dc15400c6f799d1bd71dd3f712be7d3b66ea334025b32c29b82060ad9394f798f4ab685729097edad00070f0
   - SHA256:f20b3151e39d55784bd23b021259caa5f016df69ee04e61509342f09ec9af9c6
   - SHA1:3ac5495e220fe8f52a6757ea60092da9d989ff74 [weak]
   - MD5Sum:96c8eab43b175f5331a4321bda9d6605 [weak]
   - Filesize:1624780 [weak]
  Hashes of received file:
   - SHA512:5727ca117538edecbcc407f831b30772b5c5719a67716b5609bf9bc390a06e0c2f75b75a7cd6994df23c7bc7b1c741e7702984113dab9ad5d21a9d6a58be0e0b
   - SHA256:0060cbbc96df406cba7a1c1bad2f5ff52612352bbe97efbea30887be269a9bbc
   - SHA1:fb77ce782c7472e1f8d335e977708f4297cf9744 [weak]
   - MD5Sum:eaa9f691916a3ad4eb876b72358b572f [weak]
   - Filesize:1624780 [weak]
  Last modification reported: Mon, 28 Mar 2022 09:39:02 +0000
Get:3 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 python3.8 amd64 3.8.10-0ubuntu1~20.04.4 [387 kB]
Get:4 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libpython3.8-stdlib amd64 3.8.10-0ubuntu1~20.04.4 [1675 kB]
Get:5 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 python3.8-minimal amd64 3.8.10-0ubuntu1~20.04.4 [1899 kB]
Get:6 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libpython3.8-minimal amd64 3.8.10-0ubuntu1~20.04.4 [717 kB]
Get:7 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 rsync amd64 3.1.3-8ubuntu0.3 [318 kB]
Get:8 http://archive.ubuntu.com/ubuntu focal-updates/universe amd64 open-vm-tools amd64 2:11.3.0-2ubuntu0~ubuntu20.04.2 [647 kB]
Get:9 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 libnetplan0 amd64 0.104-0ubuntu2~20.04.1 [82.0 kB]
Get:10 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 netplan.io amd64 0.104-0ubuntu2~20.04.1 [87.9 kB]
Get:11 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 tzdata all 2022a-0ubuntu0.20.04 [294 kB]
Get:12 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 python3-twisted-bin amd64 18.9.0-11ubuntu0.20.04.2 [11.4 kB]
Get:13 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 python3-twisted all 18.9.0-11ubuntu0.20.04.2 [1933 kB]
Err:13 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 python3-twisted all 18.9.0-11ubuntu0.20.04.2
  Hash Sum mismatch
  Hashes of expected file:
   - SHA512:a81c1a63549bbd42661e7ebfa99d5edb84e037b0964f0fa27cec3bb818701ba8985d8a6f8574cedaa4e380f56a3d1e06723d81a180f4796fc552d1c524aab12b
   - SHA256:2664522fc2042265f904fbecb39d461b6f5781b7810b6fed600b905804a77f7d
   - SHA1:a90df9ce91d0d1c1cc2a20440dfec3aa42b64c08 [weak]
   - MD5Sum:c91f5f619485e61bb0f2efd954f2802e [weak]
   - Filesize:1933148 [weak]
  Hashes of received file:
   - SHA512:6a6dcfa43639f761a412fece80b2dba1a19a7326993930fd50885440dcfd80767b4490b453af2809df9727a5538f22797e8c468492a9eda410b24e3068dd2159
   - SHA256:50a14607e7968ded858150626b4de65569ad81aa0967b9054fd3fcfffb5b815d
   - SHA1:c6060f6f53257cdf0e296ba2a52ba7de121bafa2 [weak]
   - MD5Sum:2b6f4f077c97b47eec48555d8e193b2b [weak]
   - Filesize:1933148 [weak]
  Last modification reported: Wed, 30 Mar 2022 05:33:29 +0000
Get:14 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 landscape-common amd64 19.12-0ubuntu4.3 [86.4 kB]
Get:15 http://archive.ubuntu.com/ubuntu focal-updates/main amd64 cloud-init all 22.1-14-g2e17a0d6-0ubuntu1~20.04.3 [483 kB]
Fetched 10.3 MB in 2s (6359 kB/s)
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/p/python3.8/libpython3.8_3.8.10-0ubuntu1~20.04.4_amd64.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:beabfa893ddbff17975f605fdd7caad35ea07b39dc15400c6f799d1bd71dd3f712be7d3b66ea334025b32c29b82060ad9394f798f4ab685729097edad00070f0
    - SHA256:f20b3151e39d55784bd23b021259caa5f016df69ee04e61509342f09ec9af9c6
    - SHA1:3ac5495e220fe8f52a6757ea60092da9d989ff74 [weak]
    - MD5Sum:96c8eab43b175f5331a4321bda9d6605 [weak]
    - Filesize:1624780 [weak]
   Hashes of received file:
    - SHA512:5727ca117538edecbcc407f831b30772b5c5719a67716b5609bf9bc390a06e0c2f75b75a7cd6994df23c7bc7b1c741e7702984113dab9ad5d21a9d6a58be0e0b
    - SHA256:0060cbbc96df406cba7a1c1bad2f5ff52612352bbe97efbea30887be269a9bbc
    - SHA1:fb77ce782c7472e1f8d335e977708f4297cf9744 [weak]
    - MD5Sum:eaa9f691916a3ad4eb876b72358b572f [weak]
    - Filesize:1624780 [weak]
   Last modification reported: Mon, 28 Mar 2022 09:39:02 +0000
E: Failed to fetch http://archive.ubuntu.com/ubuntu/pool/main/t/twisted/python3-twisted_18.9.0-11ubuntu0.20.04.2_all.deb  Hash Sum mismatch
   Hashes of expected file:
    - SHA512:a81c1a63549bbd42661e7ebfa99d5edb84e037b0964f0fa27cec3bb818701ba8985d8a6f8574cedaa4e380f56a3d1e06723d81a180f4796fc552d1c524aab12b
    - SHA256:2664522fc2042265f904fbecb39d461b6f5781b7810b6fed600b905804a77f7d
    - SHA1:a90df9ce91d0d1c1cc2a20440dfec3aa42b64c08 [weak]
    - MD5Sum:c91f5f619485e61bb0f2efd954f2802e [weak]
    - Filesize:1933148 [weak]
   Hashes of received file:
    - SHA512:6a6dcfa43639f761a412fece80b2dba1a19a7326993930fd50885440dcfd80767b4490b453af2809df9727a5538f22797e8c468492a9eda410b24e3068dd2159
    - SHA256:50a14607e7968ded858150626b4de65569ad81aa0967b9054fd3fcfffb5b815d
    - SHA1:c6060f6f53257cdf0e296ba2a52ba7de121bafa2 [weak]
    - MD5Sum:2b6f4f077c97b47eec48555d8e193b2b [weak]
    - Filesize:1933148 [weak]
   Last modification reported: Wed, 30 Mar 2022 05:33:29 +0000
E: Unable to fetch some archives, maybe run apt-get update or try with --fix-missing?
```
