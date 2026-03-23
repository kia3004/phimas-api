package com.phimas.bhwmobile.data

import android.content.Context
import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.UserSession

class SessionStore(context: Context) {
    private val preferences = context.getSharedPreferences(PREFERENCES_NAME, Context.MODE_PRIVATE)

    fun save(profile: BhwProfileDto) {
        preferences.edit()
            .putInt(KEY_USER_ID, profile.userId)
            .putString(KEY_USERNAME, profile.username)
            .apply()
    }

    fun load(): UserSession? {
        val userId = preferences.getInt(KEY_USER_ID, -1)
        val username = preferences.getString(KEY_USERNAME, null)
        if (userId <= 0 || username.isNullOrBlank()) {
            return null
        }

        return UserSession(userId = userId, username = username)
    }

    fun clear() {
        preferences.edit().clear().apply()
    }

    fun clearSession() {
        preferences.edit()
            .remove(KEY_USER_ID)
            .remove(KEY_USERNAME)
            .apply()
    }

    fun saveApiBaseUrl(baseUrl: String) {
        preferences.edit()
            .putString(KEY_API_BASE_URL, baseUrl)
            .apply()
    }

    fun loadApiBaseUrl(): String? {
        return preferences.getString(KEY_API_BASE_URL, null)
            ?.trim()
            ?.takeIf { it.isNotBlank() }
    }

    private companion object {
        private const val PREFERENCES_NAME = "bhw_mobile_session"
        private const val KEY_USER_ID = "user_id"
        private const val KEY_USERNAME = "username"
        private const val KEY_API_BASE_URL = "api_base_url"
    }
}
