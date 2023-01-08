import 'package:flutter/material.dart';

class ImageBlock extends StatefulWidget {
  const ImageBlock({super.key});

  @override
  State<ImageBlock> createState() => _ImageBlockState();
}

class _ImageBlockState extends State<ImageBlock> {
  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.yellow[700],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text('Image'),
              Expanded(
                child: Align(
                  alignment: Alignment.centerRight,
                  child: Icon(Icons.add),
                ),
              ),
            ],
          ),
          onPressed: () {
            showDialog(
              context: context,
              barrierDismissible: true,
              builder: (_) => AlertDialog(
                title: Text('asdasd'),
              ),
            );
          },
        ),
        SizedBox(height: 6),
        AspectRatio(
          aspectRatio: 16.0 / 9.0,
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.all(Radius.circular(6)),
            ),
            clipBehavior: Clip.antiAlias,
            child: Image.network(
              'https://media.istockphoto.com/id/1214625216/photo/elephant-with-a-zebra-skin-walking-in-savannah-this-is-a-3d-render-illustration.jpg?b=1&s=170667a&w=0&k=20&c=rRpDFrSK4uVCq73R2AaevGfq5RkqSq0MRZZ_RnYxLX8=',
              fit: BoxFit.cover,
            ),
          ),
        ),
      ],
    );
  }
}
