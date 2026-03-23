package com.phimas.bhwmobile.ui

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val LightColors = lightColorScheme(
    primary = Color(0xFF0F766E),
    onPrimary = Color.White,
    primaryContainer = Color(0xFFCCFBF1),
    onPrimaryContainer = Color(0xFF08312D),
    secondary = Color(0xFF2563EB),
    onSecondary = Color.White,
    tertiary = Color(0xFFB45309),
    background = Color(0xFFF3F7F6),
    surface = Color.White,
    onSurface = Color(0xFF14231F),
    surfaceVariant = Color(0xFFE5F0ED),
    outline = Color(0xFF96AAA5),
)

private val DarkColors = darkColorScheme(
    primary = Color(0xFF5EEAD4),
    onPrimary = Color(0xFF07332F),
    primaryContainer = Color(0xFF115E59),
    secondary = Color(0xFF93C5FD),
    tertiary = Color(0xFFFBBF24),
    background = Color(0xFF0C1413),
    surface = Color(0xFF111D1A),
    onSurface = Color(0xFFE6F2EF),
    surfaceVariant = Color(0xFF1A2B28),
    outline = Color(0xFF7C918B),
)

@Composable
fun BhwMobileTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = if (androidx.compose.foundation.isSystemInDarkTheme()) DarkColors else LightColors,
        typography = Typography(),
        content = content,
    )
}
