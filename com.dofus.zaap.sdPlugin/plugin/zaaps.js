const ZAAPS = [
  // Astrub & Profondeurs
  { id: "z01", name: "Cité d'Astrub", area: "Astrub", group: "Astrub & Amakna", x: 5, y: -18, coords: "[5, -18]", command: "/zaap 5,-18" },
  { id: "z02", name: "Tainéla", area: "Astrub", group: "Astrub & Amakna", x: 1, y: -32, coords: "[1, -32]", command: "/zaap 1,-32" },
  { id: "z03", name: "Arche de Vili", area: "Profondeurs d'Astrub", group: "Astrub & Amakna", x: 15, y: -20, coords: "[15, -20]", command: "/zaap 15,-20" },

  // Amakna
  { id: "z04", name: "Village d'Amakna", area: "Amakna", group: "Astrub & Amakna", x: -2, y: 0, coords: "[-2, 0]", command: "/zaap -2,0" },
  { id: "z05", name: "Château d'Amakna", area: "Amakna", group: "Astrub & Amakna", x: 3, y: -5, coords: "[3, -5]", command: "/zaap 3,-5" },
  { id: "z06", name: "Coin des Bouftous", area: "Amakna", group: "Astrub & Amakna", x: 5, y: 7, coords: "[5, 7]", command: "/zaap 5,7" },
  { id: "z07", name: "Port de Madrestam", area: "Amakna", group: "Astrub & Amakna", x: 7, y: -4, coords: "[7, -4]", command: "/zaap 7,-4" },
  { id: "z08", name: "Montagne des Craqueleurs", area: "Amakna", group: "Astrub & Amakna", x: -5, y: -8, coords: "[-5, -8]", command: "/zaap -5,-8" },
  { id: "z09", name: "Bord de la forêt maléfique", area: "Amakna", group: "Astrub & Amakna", x: -1, y: 13, coords: "[-1, 13]", command: "/zaap -1,13" },
  { id: "z10", name: "Plaine des Scarafeuilles", area: "Amakna", group: "Astrub & Amakna", x: -1, y: 24, coords: "[-1, 24]", command: "/zaap -1,24" },

  // Bonta & Brâkmar
  { id: "z11", name: "Bonta (Cœur immaculé)", area: "Bonta", group: "Cités & Cania", x: -31, y: -56, coords: "[-31, -56]", command: "/zaap -31,-56" },
  { id: "z12", name: "Brâkmar (La Cuirasse)", area: "Brâkmar", group: "Cités & Cania", x: -26, y: 37, coords: "[-26, 37]", command: "/zaap -26,37" },

  // Plaines de Cania & Foire du Trool
  { id: "z13", name: "Champs de Cania", area: "Plaines de Cania", group: "Cités & Cania", x: -27, y: -36, coords: "[-27, -36]", command: "/zaap -27,-36" },
  { id: "z14", name: "Lac de Cania", area: "Plaines de Cania", group: "Cités & Cania", x: -3, y: -42, coords: "[-3, -42]", command: "/zaap -3,-42" },
  { id: "z15", name: "Massif de Cania", area: "Plaines de Cania", group: "Cités & Cania", x: -13, y: -28, coords: "[-13, -28]", command: "/zaap -13,-28" },
  { id: "z16", name: "Plaine des Porkass", area: "Plaines de Cania", group: "Cités & Cania", x: -5, y: -23, coords: "[-5, -23]", command: "/zaap -5,-23" },
  { id: "z17", name: "Plaines Rocheuses", area: "Plaines de Cania", group: "Cités & Cania", x: -17, y: -47, coords: "[-17, -47]", command: "/zaap -17,-47" },
  { id: "z18", name: "Routes Rocailleuses", area: "Plaines de Cania", group: "Cités & Cania", x: -20, y: -20, coords: "[-20, -20]", command: "/zaap -20,-20" },
  { id: "z19", name: "Village des Dopeuls", area: "Plaines de Cania", group: "Cités & Cania", x: -34, y: -8, coords: "[-34, -8]", command: "/zaap -34,-8" },
  { id: "z20", name: "Village des Kanigs", area: "Plaines de Cania", group: "Cités & Cania", x: 0, y: -56, coords: "[0, -56]", command: "/zaap 0,-56" },
  { id: "z21", name: "Foire du Trool", area: "Foire du Trool", group: "Cités & Cania", x: -11, y: -36, coords: "[-11, -36]", command: "/zaap -11,-36" },

  // Landes de Sidimote & Koalaks
  { id: "z22", name: "Route des Roulottes", area: "Landes de Sidimote", group: "Sidimote, Koalaks & Sufokia", x: -25, y: 12, coords: "[-25, 12]", command: "/zaap -25,12" },
  { id: "z23", name: "Terres Désacrées", area: "Landes de Sidimote", group: "Sidimote, Koalaks & Sufokia", x: -15, y: 25, coords: "[-15, 25]", command: "/zaap -15,25" },
  { id: "z24", name: "Village des Éleveurs", area: "Montagne des Koalaks", group: "Sidimote, Koalaks & Sufokia", x: -16, y: 1, coords: "[-16, 1]", command: "/zaap -16,1" },
  { id: "z25", name: "Cimetière primitif", area: "Montagne des Koalaks", group: "Sidimote, Koalaks & Sufokia", x: -12, y: 19, coords: "[-12, 19]", command: "/zaap -12,19" },

  // Baie de Sufokia
  { id: "z26", name: "Sufokia", area: "Baie de Sufokia", group: "Sidimote, Koalaks & Sufokia", x: 13, y: 26, coords: "[13, 26]", command: "/zaap 13,26" },
  { id: "z27", name: "Rivage sufokien", area: "Baie de Sufokia", group: "Sidimote, Koalaks & Sufokia", x: 10, y: 22, coords: "[10, 22]", command: "/zaap 10,22" },
  { id: "z28", name: "Temple des alliances", area: "Baie de Sufokia", group: "Sidimote, Koalaks & Sufokia", x: 13, y: 35, coords: "[13, 35]", command: "/zaap 13,35" },

  // Île de Frigost & Valonia
  { id: "z29", name: "La Bourgade", area: "Île de Frigost", group: "Frigost & Archipels", x: -78, y: -41, coords: "[-78, -41]", command: "/zaap -78,-41" },
  { id: "z30", name: "Village enseveli", area: "Île de Frigost", group: "Frigost & Archipels", x: -77, y: -73, coords: "[-77, -73]", command: "/zaap -77,-73" },
  { id: "z31", name: "Entrée château Harebourg", area: "Île de Frigost", group: "Frigost & Archipels", x: -67, y: -77, coords: "[-67, -77]", command: "/zaap -67,-77" },
  { id: "z32", name: "Futaie enneigée", area: "Archipel de Valonia", group: "Frigost & Archipels", x: 39, y: -82, coords: "[39, -82]", command: "/zaap 39,-82" },

  // Îles (Pandala, Otomaï, Wabbits, Moon, etc.)
  { id: "z33", name: "Village de Pandala", area: "Île de Pandala", group: "Îles & Extensions", x: 20, y: -29, coords: "[20, -29]", command: "/zaap 20,-29" },
  { id: "z34", name: "Mont des Tombeaux", area: "Île de Grobe", group: "Îles & Extensions", x: 40, y: -44, coords: "[40, -44]", command: "/zaap 40,-44" },
  { id: "z35", name: "Village côtier", area: "Île d'Otomaï", group: "Îles & Extensions", x: -46, y: 18, coords: "[-46, 18]", command: "/zaap -46,18" },
  { id: "z36", name: "Village de la Canopée", area: "Île d'Otomaï", group: "Îles & Extensions", x: -54, y: 16, coords: "[-54, 16]", command: "/zaap -54,16" },
  { id: "z37", name: "Île de la Cawotte", area: "Île des Wabbits", group: "Îles & Extensions", x: 25, y: -4, coords: "[25, -4]", command: "/zaap 25,-4" },
  { id: "z38", name: "Laboratoires abandonnés", area: "Île des Wabbits", group: "Îles & Extensions", x: 27, y: -14, coords: "[27, -14]", command: "/zaap 27,-14" },
  { id: "z39", name: "Plage de la Tortue", area: "Île de Moon", group: "Îles & Extensions", x: 35, y: 12, coords: "[35, 12]", command: "/zaap 35,12" },
  { id: "z40", name: "Dunes des ossements", area: "Saharach", group: "Îles & Extensions", x: 15, y: -58, coords: "[15, -58]", command: "/zaap 15,-58" },
  { id: "z41", name: "Crocuzko", area: "Archipel des Écailles", group: "Îles & Extensions", x: -83, y: -15, coords: "[-83, -15]", command: "/zaap -83,-15" },
  { id: "z42", name: "Nimotopia", area: "Nimotopia", group: "Îles & Extensions", x: -67, y: 29, coords: "[-67, 29]", command: "/zaap -67,29" },

  // Incarnam
  { id: "z43", name: "Route des âmes", area: "Incarnam", group: "Incarnam", x: -1, y: -3, coords: "[-1, -3]", command: "/zaap -1,-3" },
  { id: "z44", name: "Pâturages", area: "Incarnam", group: "Incarnam", x: 2, y: -5, coords: "[2, -5]", command: "/zaap 2,-5" },
  { id: "z45", name: "Cimetière", area: "Incarnam", group: "Incarnam", x: 3, y: 0, coords: "[3, 0]", command: "/zaap 3,0" }
];

function getZaapById(id) {
  if (!id) return ZAAPS[0];
  return ZAAPS.find(z => z.id === id || `zaap_${z.x}_${z.y}` === id) || ZAAPS[0];
}

module.exports = {
  ZAAPS,
  getZaapById
};
