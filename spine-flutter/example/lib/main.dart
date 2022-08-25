import 'package:flutter/material.dart';
import 'package:spine_flutter/spine_flutter.dart';

class ExampleSelector extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    const spacer = SizedBox(height: 10);

    return Scaffold(
        appBar: AppBar(title: const Text('Spine Examples')),
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ElevatedButton(
                child: const Text('Spineboy'),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute<void>(
                      builder: (context) => const Spineboy(),
                    ),
                  );
                },
              ),
              spacer
            ]
          )
        )
    );
  }
}

class Spineboy extends StatelessWidget {
  const Spineboy({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Spineboy')),
      body: const Center(
        child: SpineWidget("assets/spineboy-pro.skel", "assets/spineboy.atlas")
      ),
    );
  }
}

void main() {
  runApp(MaterialApp(
      title: "Spine Examples",
      home: Spineboy()
  ));
}
