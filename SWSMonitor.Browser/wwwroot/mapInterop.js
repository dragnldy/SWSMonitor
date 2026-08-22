export function createAndInitializeMap(elementId, lat, lng, zoom, xoffset, yoffset, width, height) {
    // Find the position of the nativehost container in the DOM
    // This is important to ensure the map div is positioned correctly relative to the Avalonia native host
    const nativeHostContainer = document.querySelector('.avalonia-native-host');
    if (!nativeHostContainer) {
        console.error("Native host container not found. Ensure that the Avalonia native host is present in the DOM.");
        return null;
    }
    // 1. Create the native HTML div container directly in JS
    const mapDiv = document.createElement('div');
    nativeHostContainer.appendChild(mapDiv);
    console.log("Creating div for Leaflet map with ID:", elementId);
    mapDiv.id = elementId;
    mapDiv.style.position = 'relative';
    mapDiv.style.top = `${yoffset}px`;
    mapDiv.style.left = `${xoffset}px`;
    mapDiv.style.width = `${width}px`;
    mapDiv.style.height = `${height}px`;
    mapDiv.style.zIndex = '999'; // Ensure it appears above other elements
    mapDiv.style.backgroundColor = 'transparent'; // Optional: Set a background color for visibility

    // 2. Initialize Leaflet onto the created element layout
    // A short timeout gives Avalonia time to finish embedding the element into the layout frame
    // 2. Wrap initialization in a safety check
    setTimeout(() => {
        // Double-check if Leaflet has loaded onto the window global scope yet
        if (typeof L === 'undefined') {
            console.error("Leaflet (L) is still not defined. Retrying in 100ms...");
            // Fallback retry if the network or loading was slow
            setTimeout(() => initializeMapEngine(elementId, lat, lng, zoom), 100);
            return;
        }
        initializeMapEngine(elementId, lat, lng, zoom);
    }, 50);

    //3. Return the HTML element reference.
    // .NET automatically marshals this as a valid C# JSObject handle!
    return mapDiv;
}
function initializeMapEngine(elementId, lat, lng, zoom) {
    try {
        const map = L.map(elementId).setView([lat, lng], zoom);
        window._leafletMap = map;
        console.log("writing to window._leafletMap");
        window._markers = {}; // Object to store markers by id

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);

        var legend = L.control({ position: 'topright' });
        legend.onAdd = function (map) {

            var div = L.DomUtil.create('div', 'info legend');
            var labels = ['<span style="font-size:18px"><strong>Survey Sites</strong></span>'];
            var categories = ['Selected', 'Monitored', 'Inactive'];

            for (var i = 0; i < categories.length; i++) {
                var color = "black";
                switch (categories[i]) {
                    case "Selected":
                        color = 'blue';
                        break;
                    case "Monitored":
                        color = 'green';
                        break;
                    case "Inactive":
                        color = 'darkred';
                        break;
                }
                labels.push(
                    '<i class="bi bi-circle-fill" style="font-size:18px;color:' + color + '"/><span style="margin: 10px">' +
                    (categories[i] ? categories[i] : '+') + '</span>');

            }
            div.innerHTML = labels.join('<br>');
            return div;
        };
        legend.addTo(map);

        // Optional: Save map instance to global scope if you need to update coordinates later
        globalThis[`mapInstance_${elementId}`] = map;
    } catch (error) {
        console.error("Failed to initialize Leaflet map:", error);
    }
}
export async function show_marker(id) {
    var marker = window._markers[id];
    marker.setOpacity(1);
}
export async function hide_marker(id) {
    var marker = window._markers[id];
    marker.setOpacity(.1);
}
export async function change_marker_to_original(id) {
    var marker = window._markers[id];
    if (marker != null) {
        marker.setIcon(marker.markerIcon);
    }
}
export async function change_marker_to_selected(id) {
    var marker = window._markers[id];
    marker.setIcon(blueIcon);

    //if (marker != null) {
    //    marker.remove;
    //}

    //await add_marker(dotNetHelper, marker.lat, marker.lng, marker.isActive, marker.popupText, marker.id);
    window._leafletMap.flyTo([marker.lat, marker.lng], window._leafletMap.getZoom(), { animate: true, });
}
var redIcon = L.AwesomeMarkers.icon({
    icon: 'cone-striped',
    prefix: 'bi',
    iconColor: 'yellow',
    markerColor: 'red'
});
var greenIcon = L.AwesomeMarkers.icon({
    icon: 'star-fill',
    prefix: 'bi',
    iconColor: 'black',
    markerColor: 'green'
});
var blueIcon = L.AwesomeMarkers.icon({
    icon: 'bookmark-fill',
    prefix: 'bi',
    iconColor: 'yellow',
    markerColor: 'blue'
});

export function add_marker(elementId, lat, lng, isActive, popupText, id) {
    console.log("reading from window._leafletMap");

    const map = window._leafletMap;
    if (!map) {
        console.log("Map not initialized");
        return;
    }


    var colorIcon = isActive ? greenIcon : redIcon;

    const marker = L.marker([lat, lng], { icon: colorIcon, title: popupText }).addTo(window._leafletMap);
    marker.Id = id;
    marker.markerIcon = colorIcon;
    marker.lat = lat;
    marker.lng = lng;

    if (popupText) {
        marker.bindPopup(popupText);
    }
    window._markers[id] = marker;

    marker.on('click', function () {
        console.log('Marker clicked: ' + id + ' at ' + lat + ', ' + lng);
        // Call the .NET method when marker is clicked
        // dotNetHelper.invokeMethodAsync('OnMarkerClick', lat, lng, id);
    });
}
