import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart' as spine_flutter;
import 'package:flutter/services.dart' show rootBundle;

void main() {
  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  _MyAppState createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  late int majorVersion;
  late int minorVersion;

  @override
  void initState() {
    super.initState();
    majorVersion = spine_flutter.majorVersion();
    minorVersion = spine_flutter.minorVersion();
    
    loadSkeleton();
  }
  
  void loadSkeleton() async {
    final atlas = await spine_flutter.loadAtlas(rootBundle, "assets/spineboy.atlas");
    final skeletonData = spine_flutter.loadSkeletonDataJson(atlas, await rootBundle.loadString("assets/spineboy-pro.json"));
    final skeletonDataBinary = spine_flutter.loadSkeletonDataBinary(atlas, await rootBundle.load("assets/spineboy-pro.skel"));
  }

  @override
  Widget build(BuildContext context) {
    const textStyle = TextStyle(fontSize: 25);
    const spacerSmall = SizedBox(height: 10);
    return MaterialApp(
      home: Scaffold(
        body: SingleChildScrollView(
          child: Container(
            padding: const EdgeInsets.all(10),
            child: Column(
              children: [
                const Image(image: AssetImage("assets/spineboy.png")),
                spacerSmall,
                Text(
                  'Spine version: $majorVersion.$minorVersion',
                  style: textStyle,
                  textAlign: TextAlign.center,
                )
              ],
            ),
          ),
        ),
      ),
    );
  }
}
