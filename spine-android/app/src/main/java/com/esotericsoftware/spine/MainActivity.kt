package com.esotericsoftware.spine

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.Card
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.esotericsoftware.spine.ui.theme.SpineAndroidExamplesTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            AppContent()
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppContent() {
    val navController = rememberNavController()

    SpineAndroidExamplesTheme {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            NavHost(
                navController = navController,
                startDestination = Destination.Samples.route
            ) {
                composable(
                    Destination.Samples.route
                ) {
                    Scaffold(
                        topBar = { TopAppBar(title = { Text(text = Destination.Samples.title) }) }
                    ) { paddingValues ->
                        Samples(
                            navController,
                            listOf(
                                Destination.SimpleAnimation,
                                Destination.PlayPause
                            ),
                            paddingValues
                        )
                    }
                }

                composable(
                    Destination.SimpleAnimation.route
                ) {
                    SimpleAnimation(navController)
                }

                composable(
                    Destination.PlayPause.route
                ) {
                    PlayPause(navController)
                }
            }
        }
    }
}

@Composable
fun Samples(
    nav: NavHostController,
    samples: List<Destination>,
    paddingValues: PaddingValues
) {
    LazyColumn(
        verticalArrangement = Arrangement.spacedBy(8.dp),
        modifier = Modifier
            .padding(8.dp)
            .padding(paddingValues)
    ) {
        samples.forEach {
            item {
                Card(
                    Modifier
                        .fillMaxWidth()
                        .clickable(onClick = { nav.navigate(it.route) }),
                    shape = MaterialTheme.shapes.large
                ) {
                    Text(text = it.title, Modifier.padding(24.dp))
                }
            }
        }
    }
}

sealed class Destination(val route: String, val title: String) {
    data object Samples: Destination("samples", "Spine Android Examples")
    data object SimpleAnimation : Destination("simpleAnimation", "Simple Animation")
    data object PlayPause : Destination("playPause", "Play/Pause")
}
