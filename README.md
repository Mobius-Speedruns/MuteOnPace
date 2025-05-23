# MCSR - MuteOnPace

Reads MC instance files and speedrunigt files to determine if you are on pace, muting the relevant OBS sources automatically!

## Pre-requisites
You must have [streamer.bot](https://streamer.bot/) installed to use MuteOnPace.</br>
Once streamer.bot is installed, follow this [setup guide](https://docs.streamer.bot/get-started/setup) to connect to your Twitch account and OBS.

## Installation
1. Inside streamer.bot, click on import, then drag and drop [MCSR-MuteOnPace-v1.0](./MCSR-MuteOnPace-v1.0) file to the import string field.
2. Go to Commands tab, right click on the MCSR group name. Under Group, click Enable All.
3. Go to your twitch chat and use `!pacemanConfig` command to configure the app.
4. Splits should be specified in ms. e.g 420000 = 7 minutes. 
5. `wpstateout` filepath can be found by going to your instance in MultiMC (or equivalent), clicking Instance Folder, and navigating to `.minecraft/wpstateout.txt`.
6. `speedrunigt` folder should be located in your root user. e.g `C:/Users/yourname/speedrunigt`
7. Click **Save** and close the config window.
8. Inside streamer.bot, go to the **Actions** tab. You will need to update the following:
   - **MCSR - MuteOBSSources**
     - Remove the existing example sub-actions under ` [Update for your OBS]`. **Do not delete `Set global "paceman_muted"` action**
     - Add a new sub-action: `OBS -> Sources -> Set Source Mute State`.
     - Select the relevant scene and source you want to mute, and choose **State: mute**.
     - Repeat this step for each source you want to mute.
   - **MCSR - UnmuteOBSSources**
     - Remove the existing example sub-actions under ` [Update for your OBS]`. **Do not delete `Set global "paceman_muted"` action**
     - Add a new sub-action: `OBS -> Sources -> Set Source Mute State`.
     - Choose the same sources as above, but select **State: unmute**.

## Usage 
Muting and unmuting will occur automatically assuming you are using the correct MC instance. </br>
The following two twitch commands mute and unmute commands avaiable, which will pause the automatic checks for pace. By default, moderators and VIPs of your channel will have permissions for these. This can be updated under the Commands tab in streamerbot.
```
!mute // will mute the specified sources and pause automatic checks.
!unmute // will unmute the specified sources and begin automatic checks again.
```

If you want to remove the annoucements sent to the channel when muting/unmuting, simply remove the `Twitch Annoucne` sub-actions in `MCSR - MuteOBSSources` and `MCSR - UnmuteOBSSources`

## Author
Created by [mobius](https://www.twitch.tv/mobiusspeedruns)</br>
Feel free to reach out or contribute via issues or pull requests!