const ConsoleWindow = require("node-hide-console-window");

ConsoleWindow.hideConsole();

const net = require("net");
const colors = require("colors");
const ioHook = require("iohook");
const inquirer = require("inquirer");

var portCommand = "-c";
if (!process.argv.includes(portCommand) && process.argv.length > process.argv.indexOf(portCommand) + 1) process.exit(0);
var port = parseInt(process.argv[process.argv.indexOf(portCommand) + 1]);
var prefix = "tog";
var Typed = "";
var invalidCommand = false;
var watermark = "";
var updateConsole = false;
var HakList = {};
var whitelisted = "abcdefghijklmnopqrstuvwxyz0123456789".split("");
var resetTimer = 0;

// Thank You: https://gist.github.com/tedmiston/5935757
const client = new net.Socket();
client.connect(port, '127.0.0.1', function() {
	process.title = "PVZCheatTool Console";
	console.clear();
	ConsoleWindow.showConsole();
	client.write(parseMessageString("GetWatermark", "1"));
});

function parseMessageObject(id = " ", message = "text") {
	return { id: id, message: message };
}

function parseMessageString(id = " ", message = "text") {
	return JSON.stringify(parseMessageObject(id, message));
}

function parseHakList(haks = []) {
	finalHaks = [];
	haks.forEach(hak => {
		finalHaks.push(parseHak(hak.HakName, hak.Enabled, hak.Addresses, hak.Values));
	})
	return { Haks: finalHaks };
}

function parseHak(hakName = "", enabled = false, addresses = [], values = []) {
	return { HakName: hakName, Enabled: enabled, Addresses: addresses, Values: values };
}

client.on('data', function(data) {
	var rawStr = data.toString('utf8');
	if (isJSON(rawStr)) {
		var parsed = JSON.parse(rawStr);
		parsed = parseMessageObject(parsed.id, parsed.message);
		if (parsed.id == "ConsoleUpdate") updateConsole = parseInt(parsed.message) > 0,consoleUpdate();
		if (updateConsole) return;
		switch (parsed.id) {
			case "Initialize":
				watermark = parsed.message;
				console.log(colors.brightWhite(watermark));
				promptFolder();
				break;
			case "FolderInvalid":
				var isTrue = parseInt(parsed.message);
				if (isTrue > 0) {
					console.log(colors.white("The Folder Specified Does Not Exist!"));
					promptFolder();
				}
				break;
			case "PromptExe":
				promptExe(parsed.message);
				break;
			case "PromptExeFail":
				if (isJSON(parsed.message)) {
					var parsedMessage = JSON.parse(parsed.message);
					if (Array.isArray(parsedMessage)) {
						if (parsedMessage.length == 2) {
							console.log(colors.white("The File " + parsedMessage[1] + " Does Not Exist!"));
							promptExe(parsedMessage[0]);
						}
					}
				}
				break;
			case "PromptConfPathFnd":
				if (isJSON(parsed.message)) {
					var parsedMessage = JSON.parse(parsed.message);
					if (Array.isArray(parsedMessage)) {
						if (parsedMessage.length == 2) {
							confirmExe(parsedMessage[0], parsedMessage[1]);
						}
					}
				}
				break;
			case "ConsoleLog":
				console.log(colors.brightWhite(parsed.message));
				break;
			case "TerminateProcess":
				console.log(colors.brightWhite(parsed.message));
				setTimeout(() => {
					client.write(parseMessageString("TerminateProcess", "1"));
				}, 3000)
				break;
			case "ConsoleClear":
				var isTrue = parseInt(parsed.message);
				if (isTrue > 0) {
					console.clear();
				}
				break;
			case "SetHakList":
				if (isJSON(parsed.message)) {
					var parsedMessage = JSON.parse(parsed.message);
					parsedMessage = parseHakList(parsedMessage.Haks);
					HakList = parsedMessage;
				}
				break;
			case "ConsoleUpdate":
				break;
			default:
				console.log(colors.brightYellow("WARNING: App sent an unrecognized command!"));
				/*console.log(parsed);
				client.write(parseMessageString("HakList", "{}"));*/
				break;
		}
	}
})

client.on('close', function() {
	process.exit(0);
})

client.on('error', function() {
	process.exit(0);
})

function consoleUpdate() {
	if (updateConsole) {
		try {
			console.clear();
			process.stderr.write('\x1B[?25l');
			console.log(colors.white(watermark));
			console.log("");
			if (Array.isArray(HakList.Haks)) {
				for (i = 0; i < HakList.Haks.length; i++) {
					var isOn = HakList.Haks[i].Enabled;
					var suffix = isOn ? colors.brightGreen("On") : colors.brightRed("Off");
					console.log((i + 1).toString() + ") " + HakList.Haks[i].HakName + " " + suffix);
				}
			}
			console.log("");
			console.log("Prefix: " + prefix);
			console.log("Input: " + Typed);
			console.log("");
			if (invalidCommand) console.log(colors.brightRed("Invalid Command! Use " + prefix + " With The Number Choice Next Time."));
		} catch(e) {}
	}
}

function keyToString(key) {
	var strKey = String.fromCharCode(key).toLowerCase();
	if (whitelisted.includes(strKey)) return strKey;
	switch(key) {
		case 8:
			return " Backspace";
			break;
		case 13:
			return " Enter";
			break;
		default:
			return " NULL";
			break;
	}
}

function parseTyped() {
	Typed = Typed.trim();
	if (Typed.startsWith(prefix)) {
		invalidCommand = false;
		var parsed = parseInt(Typed.substring(3, Typed.length));
		parsed = parsed || 0;
		if (parsed < 1) parsed = 1;
		if (Array.isArray(HakList.Haks)) if (parsed > HakList.Haks.length) parsed = HakList.Haks.length;
		if (Array.isArray(HakList.Haks)) {
			if (HakList.Haks.length > 0) {
				try
				{
					HakList.Haks[parsed - 1].Enabled = !HakList.Haks[parsed - 1].Enabled;
				} catch(e) {
					parseHakList(HakList.Haks);
					HakList.Haks[parsed - 1].Enabled = !HakList.Haks[parsed - 1].Enabled;
				}
				var toSend = "1";
				var enabledList = [];
				for (i = 0; i < HakList.Haks.length; i++) {
					try {
						if (HakList.Haks[i].Enabled) {
							enabledList.push(true);
						} else {
							enabledList.push(false);
							if (i == parsed - 1) toSend = "0";
						}
					} catch(e) {
						enabledList.push(false);
					}
				}
				var enabledListStr = JSON.stringify(enabledList);
				client.write(parseMessageString("PlayOnOff", toSend));
				client.write(parseMessageString("UpdateHaks", enabledListStr));
			}
		}
	}
	else invalidCommand = true;
	Typed = "";
}

function createResetTimer() {
	consoleUpdate();
	clearTimeout(resetTimer);
	resetTimer = new setTimeout(() => {
		Typed = "";
		consoleUpdate();
	}, 3000)
}

ioHook.on("keydown", (e) => {
	if (updateConsole) {
		var converted = keyToString(e.rawcode);
		if (converted.length == 1) {
			Typed += converted;
		} else switch(converted) {
			case " Backspace":
				Typed = Typed.substring(0, Typed.length - 1);
				break;
			case " Enter":
				parseTyped();
				break;
			default:
				break;
		}
		if (converted != " NULL") createResetTimer();
	}
})

ioHook.start();

function promptFolder() {
	inquirer.prompt({
		message: colors.brightWhite("Enter Your PVZ Folder Path (Enter If Current Folder And '..' If It Is The Previous Folder):"),
		type: "input",
		name: "folPath"
	}).then(answer => {
		client.write(parseMessageString("FolderPath", answer.folPath));
	}).catch(e => {
		promptFolder();
	})
}

function promptExe(folderPath) {
	inquirer.prompt({
		message: colors.brightWhite("Enter Your PVZ File (Don't Include .exe):"),
		type: "input",
		name: "filePath"
	}).then(answer => {
		client.write(parseMessageString("ExePath", JSON.stringify([folderPath, answer.filePath, "0"])));
	}).catch(e => {
		promptExe(folderPath);
	})
}

function confirmExe(folderPath, potentiallyPVZ) {
	inquirer.prompt({
		message: "Do You Want To Use The Path We Found? (" + potentiallyPVZ + ")".brightWhite,
		type: "confirm",
		name: "confirmed"
	}).then(answer => {
		if (answer.confirmed) {
			client.write(parseMessageString("ExePath", JSON.stringify([folderPath, potentiallyPVZ, "1"])));
		} else {
			promptExe(folderPath);
		}
	}).catch(e => {
		confirmExe(folderPath, potentiallyPVZ);
	})
}

function isJSON(jsonVal) {
	try {
		JSON.parse(jsonVal);
		return true;
	} catch(e) { 
		return false; 
	}
}