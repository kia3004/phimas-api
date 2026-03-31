package com.phimas.bhwmobile.ui

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.phimas.bhwmobile.BuildConfig
import com.phimas.bhwmobile.data.ApiService
import com.phimas.bhwmobile.data.BhwRepository
import com.phimas.bhwmobile.data.BootstrapData
import com.phimas.bhwmobile.data.SessionStore
import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.ConsultationLogDto
import com.phimas.bhwmobile.model.DashboardSummaryDto
import com.phimas.bhwmobile.model.HealthRecordDto
import com.phimas.bhwmobile.model.InsightDto
import com.phimas.bhwmobile.model.PatientDirectoryDto
import com.phimas.bhwmobile.model.RecentReportDto
import com.phimas.bhwmobile.model.TaskItemDto
import com.phimas.bhwmobile.model.UserSession
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.net.UnknownHostException

data class AppUiState(
    val isInitializing: Boolean = true,
    val isAuthenticating: Boolean = false,
    val isRefreshing: Boolean = false,
    val isWorking: Boolean = false,
    val apiBaseUrl: String = BuildConfig.API_BASE_URL,
    val isDarkTheme: Boolean = false,
    val session: UserSession? = null,
    val profile: BhwProfileDto? = null,
    val dashboard: DashboardSummaryDto? = null,
    val tasks: List<TaskItemDto> = emptyList(),
    val patients: List<PatientDirectoryDto> = emptyList(),
    val healthRecords: List<HealthRecordDto> = emptyList(),
    val recentReports: List<RecentReportDto> = emptyList(),
    val consultationLogs: List<ConsultationLogDto> = emptyList(),
    val insights: List<InsightDto> = emptyList(),
    val authError: String? = null,
    val userMessage: String? = null,
)

class MainViewModel(
    private val repository: BhwRepository,
) : ViewModel() {
    private val _state = kotlinx.coroutines.flow.MutableStateFlow(
        AppUiState(
            apiBaseUrl = repository.getApiBaseUrl(),
            isDarkTheme = repository.isDarkThemeEnabled(),
        ),
    )
    val state = _state.asStateFlow()

    init {
        restoreSession()
    }

    fun login(username: String, password: String, apiBaseUrl: String) {
        viewModelScope.launch {
            val normalizedBaseUrl = try {
                repository.updateApiBaseUrl(apiBaseUrl)
            } catch (exception: IllegalArgumentException) {
                _state.value = _state.value.copy(
                    isInitializing = false,
                    isAuthenticating = false,
                    authError = "Invalid server configuration.",
                )
                return@launch
            }

            _state.value = _state.value.copy(
                isInitializing = false,
                isAuthenticating = true,
                authError = null,
                apiBaseUrl = normalizedBaseUrl,
            )

            try {
                val session = repository.login(username, password)
                _state.value = _state.value.copy(session = session)
                loadBootstrap(session.userId, silent = false)
            } catch (exception: Exception) {
                _state.value = _state.value.copy(
                    isAuthenticating = false,
                    authError = exception.toUserMessage(),
                )
            }
        }
    }

    fun logout() {
        repository.clearSession()
        _state.value = AppUiState(
            isInitializing = false,
            apiBaseUrl = repository.getApiBaseUrl(),
            isDarkTheme = repository.isDarkThemeEnabled(),
        )
    }

    fun updateApiBaseUrl(apiBaseUrl: String) {
        viewModelScope.launch {
            val normalizedBaseUrl = try {
                repository.updateApiBaseUrl(apiBaseUrl)
            } catch (exception: IllegalArgumentException) {
                _state.value = _state.value.copy(userMessage = "Invalid server URL.")
                return@launch
            }

            _state.value = _state.value.copy(
                apiBaseUrl = normalizedBaseUrl,
                userMessage = "Server settings updated.",
            )

            val session = _state.value.session
            if (session != null) {
                loadBootstrap(
                    userId = session.userId,
                    silent = true,
                    successMessage = "Connected to server.",
                )
            }
        }
    }

    fun refreshAll(silent: Boolean = false) {
        val session = _state.value.session ?: return
        viewModelScope.launch {
            loadBootstrap(session.userId, silent = silent)
        }
    }

    fun dismissMessage() {
        _state.value = _state.value.copy(userMessage = null)
    }

    fun setDarkTheme(isDarkTheme: Boolean) {
        if (_state.value.isDarkTheme == isDarkTheme) {
            return
        }

        repository.setDarkThemeEnabled(isDarkTheme)
        _state.value = _state.value.copy(isDarkTheme = isDarkTheme)
    }

    fun updateTaskStatus(taskId: Int, status: String) {
        runMutation { session ->
            repository.updateTaskStatus(
                userId = session.userId,
                taskId = taskId,
                status = status,
            ).message
        }
    }

    fun submitHealthRecord(
        patientName: String,
        contactNumber: String,
        disease: String,
        symptoms: String,
        status: String,
        dateRecorded: String,
        address: String,
        emergencyContactName: String,
        emergencyContactNumber: String,
    ) {
        runMutation { session ->
            repository.submitHealthRecord(
                userId = session.userId,
                patientName = patientName,
                contactNumber = contactNumber,
                dateRecorded = dateRecorded,
                disease = disease,
                symptoms = symptoms,
                status = status,
                address = address,
                emergencyContactName = emergencyContactName,
                emergencyContactNumber = emergencyContactNumber,
            ).message
        }
    }

    fun submitConsultation(
        patientName: String,
        contactNumber: String,
        reportType: String,
        content: String,
        dateGenerated: String,
        address: String,
        emergencyContactName: String,
        emergencyContactNumber: String,
    ) {
        runMutation { session ->
            repository.submitConsultation(
                userId = session.userId,
                patientName = patientName,
                contactNumber = contactNumber,
                reportType = reportType,
                content = content,
                dateGenerated = dateGenerated,
                address = address,
                emergencyContactName = emergencyContactName,
                emergencyContactNumber = emergencyContactNumber,
            ).message
        }
    }

    fun updateProfile(
        fullName: String,
        email: String,
        contactNumber: String,
        assignedArea: String,
    ) {
        runMutation { session ->
            repository.updateProfile(
                userId = session.userId,
                fullName = fullName,
                email = email,
                contactNumber = contactNumber,
                assignedArea = assignedArea,
            ).message
        }
    }

    fun changePassword(currentPassword: String, newPassword: String) {
        runMutation { session ->
            repository.changePassword(
                userId = session.userId,
                currentPassword = currentPassword,
                newPassword = newPassword,
            ).message
        }
    }

    fun updateAvailability(isAvailable: Boolean) {
        runMutation { session ->
            repository.updateAvailability(userId = session.userId, isAvailable = isAvailable).message
        }
    }

    fun uploadProfilePicture(imagePayload: String) {
        runMutation { session ->
            repository.uploadProfilePicture(userId = session.userId, imagePayload = imagePayload).message
        }
    }

    private fun restoreSession() {
        val savedSession = repository.getSavedSession()
        if (savedSession == null) {
            _state.value = AppUiState(
                isInitializing = false,
                apiBaseUrl = repository.getApiBaseUrl(),
                isDarkTheme = repository.isDarkThemeEnabled(),
            )
            return
        }

        _state.value = _state.value.copy(
            apiBaseUrl = repository.getApiBaseUrl(),
            session = savedSession,
        )
        refreshAll(silent = false)
    }

    private suspend fun loadBootstrap(userId: Int, silent: Boolean, successMessage: String? = null) {
        val current = _state.value
        _state.value = current.copy(
            isInitializing = !silent && current.dashboard == null,
            isAuthenticating = false,
            isRefreshing = silent || current.dashboard != null,
            isWorking = false,
            authError = null,
        )

        try {
            val bootstrap = repository.loadBootstrap(userId)
            applyBootstrap(bootstrap, successMessage)
        } catch (exception: Exception) {
            val message = exception.toUserMessage()
            val latest = _state.value
            _state.value = latest.copy(
                isInitializing = false,
                isAuthenticating = false,
                isRefreshing = false,
                isWorking = false,
                authError = if (latest.session == null) message else latest.authError,
                userMessage = if (latest.session != null) message else latest.userMessage,
            )
        }
    }

    private fun applyBootstrap(bootstrapData: BootstrapData, successMessage: String?) {
        _state.value = _state.value.copy(
            isInitializing = false,
            isAuthenticating = false,
            isRefreshing = false,
            isWorking = false,
            session = UserSession(
                userId = bootstrapData.profile.userId,
                username = bootstrapData.profile.username,
            ),
            profile = bootstrapData.profile,
            dashboard = bootstrapData.dashboard,
            tasks = bootstrapData.tasks,
            patients = bootstrapData.patients,
            healthRecords = bootstrapData.healthRecords,
            recentReports = bootstrapData.recentReports,
            consultationLogs = bootstrapData.consultationLogs,
            insights = bootstrapData.insights,
            authError = null,
            userMessage = successMessage,
        )
    }

    private fun runMutation(action: suspend (UserSession) -> String) {
        val session = _state.value.session ?: return
        viewModelScope.launch {
            _state.value = _state.value.copy(isWorking = true, authError = null)
            try {
                val message = action(session)
                loadBootstrap(session.userId, silent = true, successMessage = message)
            } catch (exception: Exception) {
                _state.value = _state.value.copy(
                    isWorking = false,
                    isRefreshing = false,
                    userMessage = exception.toUserMessage(),
                )
            }
        }
    }

    companion object {
        fun factory(context: Context): ViewModelProvider.Factory {
            val appContext = context.applicationContext
            return object : ViewModelProvider.Factory {
                @Suppress("UNCHECKED_CAST")
                override fun <T : ViewModel> create(modelClass: Class<T>): T {
                    return MainViewModel(
                        repository = BhwRepository(
                            defaultBaseUrl = BuildConfig.API_BASE_URL,
                            sessionStore = SessionStore(appContext),
                        ),
                    ) as T
                }
            }
        }
    }
}

private fun Throwable.toUserMessage(): String {
    if (this is HttpException) {
        val rawBody = response()?.errorBody()?.string().orEmpty()
        val extractedMessage = "\"message\"\\s*:\\s*\"([^\"]+)\"".toRegex()
            .find(rawBody)
            ?.groupValues
            ?.getOrNull(1)

        return when {
            !extractedMessage.isNullOrBlank() -> extractedMessage
            code() == 401 -> "Invalid credentials."
            code() == 404 -> "Requested record was not found."
            else -> "Request failed (${code()})."
        }
    }

    if (hasCause<UnknownHostException>() || hasCause<ConnectException>() || hasCause<SocketTimeoutException>()) {
        return "Cannot reach PHIMAS API. Verify the server is online or confirm your network connection."
    }

    return message?.takeIf { it.isNotBlank() } ?: "Unexpected error."
}

private inline fun <reified T : Throwable> Throwable.hasCause(): Boolean {
    var current: Throwable? = this
    while (current != null) {
        if (current is T) {
            return true
        }
        current = current.cause
    }
    return false
}
