# **Weapon Stat Shower**

WeaponStatShower is a mod that allows player to see the stats of a selected weapon and which enemy they can kill with 1 shot and where it does kill. This mod is compatible with [WeaponCustomizer](https://www.nexusmods.com/gtfo/mods/14?tab=posts&BH=1) and modded weapons (as far as I know).

## Mod Summary

This mod generates the description of the weapons from scratch so even if you have a custom weapon it will be automatically update the UI. The mods calculate the damage of the weapon and show which Sleeper can be killed in 1 shot and from what position.
You can enable which enemy to show from the configuration file located in:

- BepInEx/config/weaponStatShower.txt

and just follow the instructions.

**You don't need to reload the game to see the changes, just open/close the inventory or click on the weapon type tabs.**

## **Current stats shown:**

![example](https://i.ibb.co/dmX54YF/Weapon-Show-Stat-GTFO.png)

- FireMode of the weapon (the #n are the number of shots in burst weapons, the **Rate Of Fire** is the number of bullets shot in 1 second)
- Charge_Up (time in brakets)
- Spread Cone for Shotguns
- DMG: the base Damage of the weapon (xN of pellets for shotguns)
- CLP: the magazine size
- MAX: the max number of ammo (mag included)
- RLD: reload time in seconds
- PRCN: the precision multiplier (if invisible PRCN = 1)
- DIST: the distance from which the weapon starts dealing less damage
- HIP: the hip spread value (if invisible HIP = 0 OR shotgun)
- ADS: the aim down sight spread (if invisible ADS = 0 OR shotgun)
- STRG: stagger damage multiplier (if invisible, no stagger bonus)
- PIERC: the number of enemies the bullet can pierce

## Enemies selected

- DEFAULT: striker, shooter, scout; see above for configuration.
  - Each enemies can have 4 values [C,B,H,O]:
    - C: chest kill
    - B: back kill
    - H: head kill (from front)
    - O: occiput kill
  - The math is done considering always the max damage possible (so no distance applied and all pellets hit) and all enemies multiplier and armor when applied.
  - Burst and Auto weapons calculate the damage for just 1 hit.


## 1.4.0版本已加入中文。
以记事本方式打开“BepInEx/config/WeaponStatShower.cfg”文件
把倒数第二段最后一行的Language = English改为Language = Chinese，即可切换为中文。
祝你游戏愉快

## Disponible en español
Abre el archivo "BepInEx/config/WeaponStatShower.cfg" con el Bloc de notas y cambia `Language = English` por `Language = Spanish`.
Leyenda de muertes de un disparo: p = pecho, e = espalda, c = cabeza (de frente), n = nuca.

## **Installation**

### EASY

1. download [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)
2. search for Weapon Stat Shower and install it
3. launch modded, done!

### MANUAL

1. dowanload [BepInExPack GTFO](https://gtfo.thunderstore.io/package/BepInEx/BepInExPack_GTFO/) and extract it in the GTFO base folder
2. copy and paste the extracted mod zip file inside {GTFO_FOLDER}/BepInEx/plugins
3. done!

# THANKS

This is my first mod so huge thanks to [GTFO_TheArchive](https://github.com/AuriRex/GTFO_TheArchive) which this mod is based on a specific feature that I wanted as a standalone, then modified.