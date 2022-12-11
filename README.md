# Dnd5e converter

This is an unofficial tools for convert text files structured as in 5etools web page into Dnd5e files compatibles with Rule Set 5e Plugin for Talespire.
Currently provides:

1. Two methods to convert text files into a Dnd5e. The first one allow select multiple Json files with a single or multiple monster information into individuals Dnd5e files. The second one take the content copied on clipboard and convert it into a single Dnd5e file.

2. Three ways to load spell to use them as attacks in the Dnd5e file creation. Two of them, in a first step take the spell's information from the selected structured Json files or from the text copied into the clipboard, and convert it into a single Spell files.

## Install

Download the Dnd5eConverter.zip file and extract the content in a folder. Then execute the file called Dnd5eConverter.exe. 

## Usage

The tool have two different areas: Bestiary and Spell. The first, in left side, related to the conversion of Json files and text from clipboard to Dnd5e files and, the second, in right side, with relation to conversion to a Spell files and the usage of them in the Dnd5e files creation.

![Dnd5 converter](/assets/Dnd5eConverter.png)

In the frames at the bottom of each area the tool display information about the number Dnd5e and Spell files loaded.

### Convert to Dnd5E files

To convert a Json file, with a single or multiple character structures, in a singles Dnd5e files, one for each character, first of all, need download the Json files from, for example, 5etools Bestiary or Hombrew. Later, click on the first button called *“CONVERT JSON TO DND5E FILES”*. 

A dialog box will open to select the Json files to convert. Once one or more files are selected, the tool processed them to applied the new format compatible with Rule Set 5E Plugin and a new similar dialog box will appear to choose the output folder in which the Dnd5E files will be save, with name equal to the character’s name inside a folder called like the character’s source.

In similar way, to convert text copied to clipboard, for example, go to bestiary in 5etools web site,  select a character that you want convert and click in the icon in right corner of the character sheet to pop up windows sheet and then click on icon again in right corner to show the source data and, finally, click on *“Copy Code”*.

Go to Dnd5E converter and click on button called *“PASTE CLIPBOARD TEXT TO CONVERT INTO DND5E FILES”* and proceed the same manner as above.

### Convert to Spell files and load them

To convert a Json file, with a single or multiple spell structures, in a single Spell files, one for each spell, first you need download the Json files from 5etools Spell or Hombrew or copy into clipboard a spell following the same process that for characters. Later, click on the first or second button called *“CONVERT JSON TO SPELL FILES AND LOAD THEM”* or *“PASTE CLIPBOARD TEXT TO CONVERT JSON TO SPELL FILES AND LOAD THEM”* as appropriate.

A dialog box will open to select the Json files to convert to spell in the first case. The tool will process the files selected or the text in clipboard to applied the new format compatible with Rule Set 5E Plugin and keep them for use as spell in the Dnd5e files creation. A new dialog box will appear to choose the output folder in which the Spell files will be save, with name equal to the spell’s name inside a folder called *“healing”* when the spell effect is regains hit points or, otherwise, into a folder called according to the spell level in a folder called *“spell”*. 

To only load Spell files already created, click on *“LOAD SPELL FILES”*. A dialog box will be open to select a directory for which the Spell files, included files in subfolders, will be load to use as spells in the Dnd5e files creation.

## Input format

Json files and text in clipboard need to have a structure similar to the used in the web site 5etools. For a character this format is similar to:

```
{
        "name": character_name,
        "source": character_source,
        "ac": [
                {
                    "ac": ac_value,
                }
        ],
        "hp": {
                "formula": hp_dices,
                "average": hp_value
        },
        "speed": {
                "walk": speed_value
        },
        "str": str_value,
        "dex": dex_value,
        "con": con_value,
        "int": int_value,
        "wis": wis_value,
        "cha": cha_value,
	"save": {
                "int": save_mod,
        },
          "skill": {
                "perception": skill_mod
        },
	"resist": [
                list_resistances
        ],
	"immune": [
                list_immunities
        ],
	"vulnerable": [
                list_vulnerabilities
        ],
         "trait": [
                {
                        "name": trait_name,
                        "entries": [
                                trait_description
                        ]
                }
        ],
         "action": [
                {
                        "name": action_name,
                        "entries": [
                                action_description
                        ]
                }
        ],
         "bonus": [
                {
                        "name": bonus_name,
                        "entries": [
                                bonus_description
                        ]
                }
        ],
         "legendary": [
                {
                        "name": legendary_name,
                        "entries": [
                                legendary_description
                        ]
                }
        ],
	"spellcasting": [
                {
                        "name": "Spellcasting",
                        "headerEntries": [
                                spellcasting_desciption
                        ],
                        "will": [
			    list_will_spell                                
                        ],
		       "daily": [
			    list_daily_spell                                
                        ],
                        "spells": {
                                list_prepared_spell_by_level
                        },
                        "ability": spelcasting_ability,
                }
        ]
}
```
where:
- *character_name* is string with the character’s name.
- *character_source* is string with source book which contains the character.
- *ac_value* is an integer refers to the character’s armor class.
- *hp_formula* is a string containing the number of hit point dices plus the hit point modifier.
- *hp_value* is an integer with the character’s average number of hit point.
- *str_value*, *dex_value*, *con_value*, *int_value*, *wis_value* and *cha_value* are integers corresponding to character’s stats values related to strength, dexterity, constitution, intelligence, wisdom and charisma.
- *speed_value* is an integer with the number of feet that the character can move as action. 
- *save_mod* is a string representing the modifier applied in the saving throw.
- *skill_mod* is a string refer to the modifier in ability checks.
- *list_immunities*, *list_resistances* and *list_vulnerabilities* are, respectively, a list of string refers to the damages types whom the character is immune or vulnerable or that resist.
- *trait_name* and *trait_description* are, respectively, a string and an array of strings with the name and information about character’s traits.
- *action_name* and *action_description* are, respectively, a string and an array of strings with the information about the name and description for character’s actions.
- *bonus_name* and *bonus_description* are, respectively, a string and an array of strings with the information about the name and description for character’s bonus actions.
- *legendary_name* and *legendary_description* are, respectively, a string and an array of strings with the information about the name and description for character’s legendary actions.
- *spellcasting_description* is a string with information about the difficulty class and attack modifiers related to character’s spellcasting ability.
- *spellcasting_ability* is a string containing the character’s stat used in spellcasting.
- *list_will_spell*, *list_daily_spell* and *list_prepared_spell_by_level* are list of string containing the spells that the character can cast in innate way, daily or through the magic study, respectively.

For a spell the format is:

```
{
        "name": spell_name,
        "source": spell_source,
        "level": spell_level,
        "range": {
                "type": spell_type,
                "distance": {
                        "type": distance_type,
                        "amount": distance_number
                }
        },
        "entries": [
                spell_description
        ],
        "damageInflict": [
                list_damages
        ],
        "savingThrow": [
                list_stat
        ],
        "spellAttack": [
                list_attack
        ]
}
```
where:
- *spell_name* is string with the spell’s name.
- *spell_source* is string with source book which contains the spell.
- *spell_level* is an integer refers to the spell’s level.
- *spell_type* is a string containing the type of range for the spell: self, touch...
- *distance_type* is a string with the metric used in the calculate of the distance.
- *distance_number* is an integer that represent the range of the spell.
- *spell_description* is a string o list of strings containing information about the spell effect.
- *list_damages* is a list of string contained the damage type done by the spell
- *list_stat* is a list of statistic in which based the spell saving throws.
- *list_attack* is a list of types of attack against armor class necessaries for do the spell effect.

## Output format

Dnd5e files contains the character's characteristics structured as following:

```
{
     "NPC": npc_value,
     "hp": hp_value,     
     "ac": ac_value,
     "str": str_value,
     "dex": dex_value,
     "con": con_value,
     "int": int_value,
     "wis": wis_value,
     "cha": cha_value,
     "speed": speed_value,
     "attacks":[
          {
               "name": attack_name,
               "type": attack_type,
               "range": attack_range,
               "roll": attack_roll,
               "critrangemin": attack_critrange_value,
               "critmultip": attack_critmul_value,
               "link":{
                    "name": damage_name,
                    "roll":" damage_roll,
                    "type": damage_type
               }
          }
     ],
     "attacksDC":[
          {
               "name": attack_name,
               "type": attack_type,
               "range": attack_range,
               "roll": attack_roll,
               "link":{
                    "name": damage_name,
                    "roll": damage_roll,
                    "type": damage_type
               }
     ],
     "healing":[
	{
	    "name": healing_name,
	    "type": healing_type,
	    "roll": healing_roll,
	}

     ],
     "saves":[
          {
               "name": save_stat,
               "type": save_type,
               "roll": save_roll
          }
     ],
     "skills":[
          {
               "name": skill_name,
               "type": skill_type,
               "roll": skill_roll
          }
     ],
     "immunity":[
	list_immunities
     ],
     "resistance":[
        list_resistances
     ],
     "vulnerability":[
	list_vulnerabilities
     ]
}
```
where:
- *npc_value* is a boolean that take true value when the character contained in the dnd5e file represent a NPC (Non Player Character) or false otherwise.
- *hp_value* is a string with the character’s number of hit points.
- *ac_value* is a string containing the character’s armor class.
- *str_value*, *dex_value*, *con_value*, *int_value*, *wis_value* and *cha_value* are the corresponding character’s stats: strength, dexterity, constitution, intelligence, wisdom and charisma.
- *speed_value* is a string with the number of feet that the character can move as action. 
- *attack_name* is a string representing the name of the attack.
- *attack_type* is a string refer to the type of attack with valid values: Melee, Range, Ranged and Magic.
- *attack_range* is a string that contains the normal and maximum distance in feet since the attack can reach the target, separated by a forward slash.
- *attack_roll* is a string containing information abaout attack. When the action is an attack versus the target's armor class, the roll has the form *1D20+mod_value*, where *mod_value* is the character’s attack modifier. If the target need to success a saving throw versus a character's difficult class, then the roll are *dc_value/stat_dc/succes_value*, where *dc_value* is the value that the target need to reach to success in the saving throw, "stat_dc" are the first three characters of the stat for which the target need to saving throw and "succes_value" indicates the effect that result in case of successful saving throw, that can be: *half*, when receiving half damage or *zero* when no receive damage. A value equal to *100* is also accepted and it refers to an attack that no need roll against AC or saving throw to hit.
- *attack_critrange_value* is a string with the minimum value on roll that is consider a critical hit. 
- *attack_critmul_value* is a string with the value that multiplies the damage rolls in case of critical hit.
- *damage_name* is a string refers to a name of the attack damage.
- *damage_roll* is a string with the damage's dice that do the attack.
- *damage_type* is a string containing the type of damage done by the attack: fire, lightning, slashing, bludgeoning, force, etc.
- *healing_name* is a string representing the name of the character healing ability.
- *healing_type* is string like *attack_type*.
- *healing_roll* is a string containing the heal's dice from the ability.
- *saves_stat* is the first three characters of the character’s statistic for which do the saving throw.
- *saves_type* is a string refer to the public o private character of the saving throw.
- *saves_roll* is a string with similar form to an attack, *1D20+mod_value*, where *mod_value* is the stat's modifier.
- *skill_name* is a string with the name of the ability check.
- *skill_type* is a string refer to the public o private character of the ability check.
- *skill_roll* is a string with similar form to a saving roll, *1D20+mod_value*, where *mod_value* is the stat's modifier used by the skill check.
- *list_immunities*, *list_resistances* and *list_vulnerabilities* are, respectively, list of string refers to the damages types whom the character is immune or vulnerable or that resist.
      
Spell files are structured as the *attacks*, *attacksDC* and *healing* in the Dnd5e file.

