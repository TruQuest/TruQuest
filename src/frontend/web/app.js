async function fetchAndResizeImage(url) {
  const response = await fetch(url);
  var blob = await response.blob();
  const bitmap = await createImageBitmap(blob);

  const maxWidth = 1920.0;
  const maxHeight = 1080.0;

  console.log(`Width: ${bitmap.width}; Height: ${bitmap.height}`);

  var targetWidth = null;
  var targetHeight = null;
  if (bitmap.width > maxWidth) {
    targetWidth = maxWidth;
    const calcHeight = (targetHeight = Math.floor(
      bitmap.height * (targetWidth / bitmap.width)
    ));
    if (calcHeight > maxHeight) {
      targetHeight = maxHeight;
      targetWidth = Math.floor(targetWidth * (targetHeight / calcHeight));
    }
  } else if (bitmap.height > maxHeight) {
    targetHeight = maxHeight;
    const calcWidth = (targetWidth = Math.floor(
      bitmap.width * (targetHeight / bitmap.height)
    ));
    if (calcWidth > maxWidth) {
      targetWidth = maxWidth;
      targetHeight = Math.floor(targetHeight * (targetWidth / calcWidth));
    }
  }

  console.log(`Target width: ${targetWidth}; Target height: ${targetHeight}`);

  if (targetWidth != null && targetHeight != null) {
    const canvas = document.createElement("canvas");
    canvas.width = targetWidth;
    canvas.height = targetHeight;

    const _pica = pica();
    blob = await _pica.toBlob(await _pica.resize(bitmap, canvas), "image/jpeg");
  }

  const buffer = await blob.arrayBuffer();

  return { buffer: buffer, mimeType: blob.type };
}

async function getImagePalette(url) {
  var promise = new Promise((resolve, _) => {
    var image = new Image();
    image.crossOrigin = "Anonymous";
    image.src = url;

    image.onload = (_) => {
      console.log("Image loaded!");
      var colorThief = new ColorThief();
      var palette = colorThief.getPalette(image);
      resolve(palette);
    };
  });

  var palette = await promise;
  var colors = [];
  palette.forEach((color) => {
    colors.push({ red: color[0], green: color[1], blue: color[2] });
  });

  return { colors: colors };
}
