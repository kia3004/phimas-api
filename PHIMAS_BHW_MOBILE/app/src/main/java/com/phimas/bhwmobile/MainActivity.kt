package com.phimas.bhwmobile

import android.net.Uri
import android.os.Bundle
import android.util.Base64
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.activity.viewModels
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.outlined.Article
import androidx.compose.material.icons.automirrored.outlined.ExitToApp
import androidx.compose.material.icons.outlined.AccountCircle
import androidx.compose.material.icons.outlined.AddAPhoto
import androidx.compose.material.icons.outlined.AssignmentTurnedIn
import androidx.compose.material.icons.outlined.Favorite
import androidx.compose.material.icons.outlined.Refresh
import androidx.compose.material.icons.outlined.SpaceDashboard
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedCard
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Surface
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import coil.compose.AsyncImage
import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.ConsultationLogDto
import com.phimas.bhwmobile.model.HealthRecordDto
import com.phimas.bhwmobile.model.InsightDto
import com.phimas.bhwmobile.model.PatientDirectoryDto
import com.phimas.bhwmobile.model.RecentReportDto
import com.phimas.bhwmobile.model.TaskItemDto
import com.phimas.bhwmobile.ui.AppUiState
import com.phimas.bhwmobile.ui.BhwMobileTheme
import com.phimas.bhwmobile.ui.MainViewModel
import kotlinx.coroutines.delay
import java.time.Instant
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.OffsetDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.util.Locale

class MainActivity : ComponentActivity() {
    private val viewModel by viewModels<MainViewModel> { MainViewModel.factory(this) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        setContent {
            BhwMobileTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    BhwApp(viewModel = viewModel)
                }
            }
        }
    }
}

private enum class AppTab(
    val label: String,
    val icon: androidx.compose.ui.graphics.vector.ImageVector,
) {
    Dashboard("Dashboard", Icons.Outlined.SpaceDashboard),
    Tasks("Tasks", Icons.Outlined.AssignmentTurnedIn),
    HealthRecords("Health Records", Icons.Outlined.Favorite),
    Reports("Consultation Logs", Icons.AutoMirrored.Outlined.Article),
    Account("Account", Icons.Outlined.AccountCircle),
}

private data class ChoiceItem(
    val value: String,
    val label: String,
)

@Composable
private fun BhwApp(viewModel: MainViewModel) {
    val state by viewModel.state.collectAsStateWithLifecycle()
    val snackbarHostState = remember { SnackbarHostState() }

    LaunchedEffect(state.userMessage) {
        val message = state.userMessage ?: return@LaunchedEffect
        snackbarHostState.showSnackbar(message)
        viewModel.dismissMessage()
    }

    when {
        state.isInitializing -> LoadingScreen()
        state.session == null -> LoginScreen(
            apiBaseUrl = state.apiBaseUrl,
            isAuthenticating = state.isAuthenticating,
            errorMessage = state.authError,
            onLogin = viewModel::login,
        )

        else -> MainShell(
            state = state,
            snackbarHostState = snackbarHostState,
            onRefresh = { viewModel.refreshAll(silent = true) },
            onLogout = viewModel::logout,
            onUpdateAvailability = viewModel::updateAvailability,
            onUpdateTaskStatus = viewModel::updateTaskStatus,
            onSubmitHealthRecord = viewModel::submitHealthRecord,
            onSubmitConsultation = viewModel::submitConsultation,
            onUpdateProfile = viewModel::updateProfile,
            onChangePassword = viewModel::changePassword,
            onUploadProfilePicture = viewModel::uploadProfilePicture,
        )
    }
}

@Composable
private fun LoadingScreen() {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center,
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            CircularProgressIndicator()
            Spacer(modifier = Modifier.height(16.dp))
            Text("Loading...")
        }
    }
}

@Composable
private fun LoginScreen(
    apiBaseUrl: String,
    isAuthenticating: Boolean,
    errorMessage: String?,
    onLogin: (String, String, String) -> Unit,
) {
    var username by rememberSaveable { mutableStateOf("") }
    var password by rememberSaveable { mutableStateOf("") }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        contentAlignment = Alignment.Center,
    ) {
        Card(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(28.dp),
            colors = CardDefaults.cardColors(
                containerColor = MaterialTheme.colorScheme.surface,
            ),
        ) {
            Column(
                modifier = Modifier.padding(24.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
            ) {
                Image(
                    painter = painterResource(id = R.drawable.logop),
                    contentDescription = "Logo",
                    modifier = Modifier.size(120.dp),
                    contentScale = ContentScale.Fit
                )
                Text(
                    text = "PHIMAS BHW Mobile",
                    style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.SemiBold,
                    textAlign = TextAlign.Center
                )
                Text(
                    text = "Sign in to your account",
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    textAlign = TextAlign.Center
                )
                OutlinedTextField(
                    value = username,
                    onValueChange = { username = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Username or email") },
                    singleLine = true,
                )
                OutlinedTextField(
                    value = password,
                    onValueChange = { password = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Password") },
                    visualTransformation = PasswordVisualTransformation(),
                    singleLine = true,
                )
                if (!errorMessage.isNullOrBlank()) {
                    Text(
                        text = errorMessage,
                        color = MaterialTheme.colorScheme.error,
                        textAlign = TextAlign.Center
                    )
                }
                Button(
                    onClick = { onLogin(username, password, apiBaseUrl) },
                    modifier = Modifier.fillMaxWidth(),
                    enabled = username.isNotBlank() && password.isNotBlank() && !isAuthenticating,
                ) {
                    if (isAuthenticating) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(18.dp),
                            strokeWidth = 2.dp,
                            color = MaterialTheme.colorScheme.onPrimary,
                        )
                    } else {
                        Text("Sign In")
                    }
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun MainShell(
    state: AppUiState,
    snackbarHostState: SnackbarHostState,
    onRefresh: () -> Unit,
    onLogout: () -> Unit,
    onUpdateAvailability: (Boolean) -> Unit,
    onUpdateTaskStatus: (Int, String) -> Unit,
    onSubmitHealthRecord: (String, String, String, String, String, String, String, String, String) -> Unit,
    onSubmitConsultation: (String, String, String, String, String, String, String, String) -> Unit,
    onUpdateProfile: (String, String, String, String) -> Unit,
    onChangePassword: (String, String) -> Unit,
    onUploadProfilePicture: (String) -> Unit,
) {
    var currentTab by rememberSaveable { mutableStateOf(AppTab.Dashboard.name) }
    val selectedTab = remember(currentTab) { AppTab.valueOf(currentTab) }
    val lifecycleOwner = LocalLifecycleOwner.current

    DisposableEffect(lifecycleOwner, state.session?.userId) {
        val observer = LifecycleEventObserver { _, event ->
            if (event == Lifecycle.Event.ON_RESUME && state.session != null) {
                onRefresh()
            }
        }
        lifecycleOwner.lifecycle.addObserver(observer)
        onDispose { lifecycleOwner.lifecycle.removeObserver(observer) }
    }

    LaunchedEffect(state.session?.userId) {
        if (state.session == null) {
            return@LaunchedEffect
        }

        while (true) {
            delay(30000)
            onRefresh()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface,
                ),
                title = {
                    Column {
                        Text(selectedTab.label, maxLines = 1, overflow = TextOverflow.Ellipsis)
                        Text(
                            text = state.profile?.bhwName ?: state.dashboard?.bhwName.orEmpty(),
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                },
                actions = {
                    if (state.isRefreshing || state.isWorking) {
                        CircularProgressIndicator(
                            modifier = Modifier
                                .padding(end = 12.dp)
                                .size(20.dp),
                            strokeWidth = 2.dp,
                        )
                    } else {
                        IconButton(onClick = onRefresh) {
                            Icon(Icons.Outlined.Refresh, contentDescription = "Refresh")
                        }
                    }
                    IconButton(onClick = onLogout) {
                        Icon(Icons.AutoMirrored.Outlined.ExitToApp, contentDescription = "Logout")
                    }
                },
            )
        },
        bottomBar = {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(horizontal = 8.dp, vertical = 10.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
            ) {
                AppTab.entries.forEach { tab ->
                    TextButton(
                        onClick = { currentTab = tab.name },
                    ) {
                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                            Icon(
                                imageVector = tab.icon,
                                contentDescription = tab.label,
                                tint = if (tab == selectedTab) {
                                    MaterialTheme.colorScheme.primary
                                } else {
                                    MaterialTheme.colorScheme.onSurfaceVariant
                                },
                            )
                            Text(
                                text = tab.label,
                                color = if (tab == selectedTab) {
                                    MaterialTheme.colorScheme.primary
                                } else {
                                    MaterialTheme.colorScheme.onSurfaceVariant
                                },
                                style = MaterialTheme.typography.labelSmall,
                            )
                        }
                    }
                }
            }
        },
        snackbarHost = { SnackbarHost(hostState = snackbarHostState) },
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
        ) {
            if (state.isRefreshing || state.isWorking || state.isAuthenticating) {
                LinearProgressIndicator(modifier = Modifier.fillMaxWidth())
            }

            when (selectedTab) {
                AppTab.Dashboard -> DashboardScreen(
                    state = state,
                    onToggleAvailability = onUpdateAvailability,
                )

                AppTab.Tasks -> TasksScreen(
                    tasks = state.tasks,
                    isBusy = state.isWorking,
                    onUpdateTaskStatus = onUpdateTaskStatus,
                )

                AppTab.HealthRecords -> HealthRecordsScreen(
                    records = state.healthRecords,
                    assignedArea = state.profile?.assignedArea.orEmpty(),
                    isBusy = state.isWorking,
                    onSubmitRecord = onSubmitHealthRecord,
                )

                AppTab.Reports -> ConsultationLogsScreen(
                    logs = state.consultationLogs,
                    assignedArea = state.profile?.assignedArea.orEmpty(),
                    isBusy = state.isWorking,
                    onSubmitConsultation = onSubmitConsultation,
                )

                AppTab.Account -> AccountScreen(
                    profile = state.profile,
                    apiBaseUrl = state.apiBaseUrl,
                    isBusy = state.isWorking,
                    onUpdateProfile = onUpdateProfile,
                    onChangePassword = onChangePassword,
                    onUploadProfilePicture = onUploadProfilePicture,
                )
            }
        }
    }
}

@Composable
private fun DashboardScreen(
    state: AppUiState,
    onToggleAvailability: (Boolean) -> Unit,
) {
    val summary = state.dashboard
    val todayTasks = state.tasks.filter { isToday(it.taskDate) }

    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        item {
            SectionCard {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Text(
                            text = "Good day, ${state.profile?.bhwName ?: summary?.bhwName.orEmpty()}",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.SemiBold,
                        )
                    }
                    Spacer(modifier = Modifier.width(16.dp))
                    Column(horizontalAlignment = Alignment.End) {
                        Text("Availability", style = MaterialTheme.typography.labelMedium)
                        Switch(
                            checked = state.profile?.isAvailable ?: summary?.isAvailable ?: false,
                            onCheckedChange = onToggleAvailability,
                        )
                    }
                }
            }
        }

        if (summary != null) {
            item {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        MetricCard(
                            modifier = Modifier.weight(1f),
                            label = "Assigned",
                            value = summary.assignedTasks.toString(),
                            supporting = "Total tasks assigned to you.",
                            tint = Color(0xFF0F766E),
                        )
                        MetricCard(
                            modifier = Modifier.weight(1f),
                            label = "High Priority",
                            value = summary.priorityTasks.toString(),
                            supporting = "Urgent follow-up items.",
                            tint = Color(0xFFDC2626),
                        )
                    }
                    Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        MetricCard(
                            modifier = Modifier.weight(1f),
                            label = "Due Today",
                            value = summary.dueTasks.toString(),
                            supporting = "Today's scheduled work.",
                            tint = Color(0xFFD97706),
                        )
                        MetricCard(
                            modifier = Modifier.weight(1f),
                            label = "Completed",
                            value = "${summary.completedTasks} / ${summary.progress}%",
                            supporting = "Current completion progress.",
                            tint = Color(0xFF2563EB),
                        )
                    }
                }
            }
        }

        item {
            SectionHeading(
                title = "Today's Tasks",
                subtitle = "",
            )
        }

        if (todayTasks.isEmpty()) {
            item { EmptyStateCard("No tasks scheduled for today.") }
        } else {
            items(todayTasks, key = { it.id }) { task ->
                TaskSummaryCard(task = task)
            }
        }

        item {
            SectionHeading(
                title = "Recent Consultation Logs",
                subtitle = "Latest reports you submitted.",
            )
        }

        if (state.recentReports.isEmpty()) {
            item { EmptyStateCard("No consultation logs available yet.") }
        } else {
            items(state.recentReports.take(4), key = { it.reportId }) { report ->
                ReportCard(report = report)
            }
        }

        item {
            SectionHeading(
                title = "Health Trend Summary",
                subtitle = "",
            )
        }

        if (state.insights.isEmpty()) {
            item { EmptyStateCard("No guidance available right now.") }
        } else {
            items(state.insights.take(1), key = { it.id }) { insight ->
                InsightCard(insight = insight)
            }
        }
    }
}

@Composable
private fun TasksScreen(
    tasks: List<TaskItemDto>,
    isBusy: Boolean,
    onUpdateTaskStatus: (Int, String) -> Unit,
) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        item {
            SectionHeading(
                title = "Assigned Tasks",
                subtitle = "",
            )
        }

        if (tasks.isEmpty()) {
            item { EmptyStateCard("No assigned tasks found.") }
        } else {
            items(tasks, key = { it.id }) { task ->
                TaskEditorCard(
                    task = task,
                    isBusy = isBusy,
                    onUpdateTaskStatus = onUpdateTaskStatus,
                )
            }
        }
    }
}

@Composable
private fun HealthRecordsScreen(
    records: List<HealthRecordDto>,
    assignedArea: String,
    isBusy: Boolean,
    onSubmitRecord: (String, String, String, String, String, String, String, String, String) -> Unit,
) {
    var patientName by rememberSaveable { mutableStateOf("") }
    var contactNumber by rememberSaveable { mutableStateOf("") }
    var disease by rememberSaveable { mutableStateOf("") }
    var symptoms by rememberSaveable { mutableStateOf("") }
    var status by rememberSaveable { mutableStateOf("Submitted") }
    var dateRecorded by rememberSaveable { mutableStateOf(LocalDate.now().toString()) }
    var address by rememberSaveable { mutableStateOf("") }
    var emergencyContactName by rememberSaveable { mutableStateOf("") }
    var emergencyContactNumber by rememberSaveable { mutableStateOf("") }

    LaunchedEffect(assignedArea) {
        if (address.isBlank()) {
            address = assignedArea.trim()
        }
    }

    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        item {
            SectionHeading(
                title = "Submit Health Record",
                subtitle = "Create patient and household records.",
            )
        }

        item {
            SectionCard {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    OutlinedTextField(
                        value = patientName,
                        onValueChange = { patientName = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Patient Name") },
                        placeholder = { Text("Enter patient name") },
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = contactNumber,
                        onValueChange = { contactNumber = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Contact Number") },
                        placeholder = { Text("Enter patient contact number") },
                        singleLine = true,
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                    )
                    OutlinedTextField(
                        value = dateRecorded,
                        onValueChange = { dateRecorded = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Date Recorded") },
                        placeholder = { Text("YYYY-MM-DD") },
                        singleLine = true,
                    )
                    InlineHouseholdDetailsCard(
                        address = address,
                        onAddressChange = { address = it },
                        emergencyContactName = emergencyContactName,
                        onEmergencyContactNameChange = { emergencyContactName = it },
                        emergencyContactNumber = emergencyContactNumber,
                        onEmergencyContactNumberChange = { emergencyContactNumber = it },
                    )
                    OutlinedTextField(
                        value = disease,
                        onValueChange = { disease = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Disease") },
                        singleLine = true,
                    )
                    DropdownField(
                        label = "Status",
                        selectedValue = status,
                        options = listOf("Submitted", "Active", "Monitoring", "Escalated").map {
                            ChoiceItem(value = it, label = it)
                        },
                        valueLabel = { it.ifBlank { "Status" } },
                        onSelected = { status = it.value },
                    )
                    OutlinedTextField(
                        value = symptoms,
                        onValueChange = { symptoms = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Symptoms") },
                        minLines = 4,
                    )
                    Button(
                        onClick = {
                            onSubmitRecord(
                                patientName.trim(),
                                contactNumber.trim(),
                                disease.trim(),
                                symptoms.trim(),
                                status,
                                dateRecorded.trim(),
                                address.trim(),
                                emergencyContactName.trim(),
                                emergencyContactNumber.trim(),
                            )
                            patientName = ""
                            contactNumber = ""
                            disease = ""
                            symptoms = ""
                            status = "Submitted"
                            dateRecorded = LocalDate.now().toString()
                            address = assignedArea.trim()
                            emergencyContactName = ""
                            emergencyContactNumber = ""
                        },
                        enabled = !isBusy &&
                            patientName.isNotBlank() &&
                            contactNumber.isNotBlank() &&
                            disease.isNotBlank() &&
                            symptoms.isNotBlank() &&
                            dateRecorded.isNotBlank() &&
                            address.isNotBlank(),
                    ) {
                        Text("Submit Record")
                    }
                }
            }
        }

        item {
            SectionHeading(
                title = "My Submitted Records",
                subtitle = "Latest entries.",
            )
        }

        if (records.isEmpty()) {
            item { EmptyStateCard("No health records submitted yet.") }
        } else {
            items(records, key = { it.recordId }) { record ->
                HealthRecordCard(record = record)
            }
        }
    }
}

@Composable
private fun InlineHouseholdDetailsCard(
    address: String,
    onAddressChange: (String) -> Unit,
    emergencyContactName: String,
    onEmergencyContactNameChange: (String) -> Unit,
    emergencyContactNumber: String,
    onEmergencyContactNumberChange: (String) -> Unit,
) {
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            OutlinedTextField(
                value = address,
                onValueChange = onAddressChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Household Address") },
                placeholder = { Text("Barangay, street, house/unit, landmark") },
                minLines = 3,
            )
            OutlinedTextField(
                value = emergencyContactName,
                onValueChange = onEmergencyContactNameChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Emergency Contact Name") },
                placeholder = { Text("Optional emergency contact name") },
                singleLine = true,
            )
            OutlinedTextField(
                value = emergencyContactNumber,
                onValueChange = onEmergencyContactNumberChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Emergency Contact Number") },
                placeholder = { Text("Optional contact number") },
                singleLine = true,
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
            )
        }
    }
}

@Composable
private fun ConsultationLogsScreen(
    logs: List<ConsultationLogDto>,
    assignedArea: String,
    isBusy: Boolean,
    onSubmitConsultation: (String, String, String, String, String, String, String, String) -> Unit,
) {
    var patientName by rememberSaveable { mutableStateOf("") }
    var contactNumber by rememberSaveable { mutableStateOf("") }
    var content by rememberSaveable { mutableStateOf("") }
    var dateGenerated by rememberSaveable { mutableStateOf(LocalDate.now().toString()) }
    var address by rememberSaveable { mutableStateOf("") }
    var emergencyContactName by rememberSaveable { mutableStateOf("") }
    var emergencyContactNumber by rememberSaveable { mutableStateOf("") }

    LaunchedEffect(assignedArea) {
        if (address.isBlank()) {
            address = assignedArea.trim()
        }
    }

    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        item {
            SectionHeading(
                title = "Submit Consultation Log",
                subtitle = "Enter a consultation log manually.",
            )
        }

        item {
            SectionCard {
                Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    OutlinedTextField(
                        value = patientName,
                        onValueChange = { patientName = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Patient Name") },
                        placeholder = { Text("Enter patient name") },
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = contactNumber,
                        onValueChange = { contactNumber = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Contact Number") },
                        placeholder = { Text("Enter patient contact number") },
                        singleLine = true,
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                    )
                    OutlinedTextField(
                        value = dateGenerated,
                        onValueChange = { dateGenerated = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Date Generated") },
                        placeholder = { Text("YYYY-MM-DD") },
                        singleLine = true,
                    )
                    InlineHouseholdDetailsCard(
                        address = address,
                        onAddressChange = { address = it },
                        emergencyContactName = emergencyContactName,
                        onEmergencyContactNameChange = { emergencyContactName = it },
                        emergencyContactNumber = emergencyContactNumber,
                        onEmergencyContactNumberChange = { emergencyContactNumber = it },
                    )
                    OutlinedTextField(
                        value = "Consultation Log",
                        onValueChange = {},
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Record Type") },
                        readOnly = true,
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = content,
                        onValueChange = { content = it },
                        modifier = Modifier.fillMaxWidth(),
                        label = { Text("Consultation Summary") },
                        minLines = 5,
                    )
                    Button(
                        onClick = {
                            onSubmitConsultation(
                                patientName.trim(),
                                contactNumber.trim(),
                                "Consultation Log",
                                content.trim(),
                                dateGenerated.trim(),
                                address.trim(),
                                emergencyContactName.trim(),
                                emergencyContactNumber.trim(),
                            )
                            patientName = ""
                            contactNumber = ""
                            content = ""
                            dateGenerated = LocalDate.now().toString()
                            address = assignedArea.trim()
                            emergencyContactName = ""
                            emergencyContactNumber = ""
                        },
                        enabled = !isBusy &&
                            patientName.isNotBlank() &&
                            contactNumber.isNotBlank() &&
                            content.isNotBlank() &&
                            dateGenerated.isNotBlank() &&
                            address.isNotBlank(),
                    ) {
                        Text("Submit Consultation Log")
                    }
                }
            }
        }

        item {
            SectionHeading(
                title = "Saved Consultation Logs",
                subtitle = "Historical submissions.",
            )
        }

        if (logs.isEmpty()) {
            item { EmptyStateCard("No consultation logs available yet.") }
        } else {
            items(logs, key = { it.reportId }) { log ->
                ConsultationLogCard(log = log)
            }
        }
    }
}

@Composable
private fun AccountScreen(
    profile: BhwProfileDto?,
    apiBaseUrl: String,
    isBusy: Boolean,
    onUpdateProfile: (String, String, String, String) -> Unit,
    onChangePassword: (String, String) -> Unit,
    onUploadProfilePicture: (String) -> Unit,
) {
    val context = LocalContext.current
    var fullName by rememberSaveable { mutableStateOf("") }
    var email by rememberSaveable { mutableStateOf("") }
    var contactNumber by rememberSaveable { mutableStateOf("") }
    var assignedArea by rememberSaveable { mutableStateOf("") }
    var currentPassword by rememberSaveable { mutableStateOf("") }
    var newPassword by rememberSaveable { mutableStateOf("") }

    LaunchedEffect(
        profile?.userId,
        profile?.bhwName,
        profile?.email,
        profile?.contactNumber,
        profile?.assignedArea,
        apiBaseUrl,
    ) {
        fullName = profile?.bhwName.orEmpty()
        email = profile?.email.orEmpty()
        contactNumber = profile?.contactNumber.orEmpty()
        assignedArea = profile?.assignedArea.orEmpty()
    }

    val launcher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        uri ?: return@rememberLauncherForActivityResult
        val encodedImage = context.encodeUriToBase64(uri) ?: return@rememberLauncherForActivityResult
        onUploadProfilePicture(encodedImage)
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        SectionHeading(
            title = "Profile",
            subtitle = "",
        )

        SectionCard {
            Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Text(
                            text = profile?.bhwName ?: "BHW Profile",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.SemiBold,
                        )
                        Text(
                            text = profile?.email.orEmpty(),
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                    AsyncImage(
                        model = resolveImageUrl(profile?.profilePicture, apiBaseUrl),
                        contentDescription = "Profile picture",
                        modifier = Modifier
                            .size(72.dp)
                            .clip(CircleShape)
                            .background(MaterialTheme.colorScheme.surfaceVariant),
                        contentScale = ContentScale.Crop,
                    )
                }

                OutlinedTextField(
                    value = fullName,
                    onValueChange = { fullName = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Full Name") },
                    singleLine = true,
                )
                OutlinedTextField(
                    value = email,
                    onValueChange = { email = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Email") },
                    singleLine = true,
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                )
                OutlinedTextField(
                    value = contactNumber,
                    onValueChange = { contactNumber = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Contact Number") },
                    singleLine = true,
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                )
                OutlinedTextField(
                    value = assignedArea,
                    onValueChange = { assignedArea = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Assigned Area") },
                    singleLine = true,
                )

                Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    Button(
                        onClick = { onUpdateProfile(fullName, email, contactNumber, assignedArea) },
                        enabled = !isBusy && fullName.isNotBlank() && email.isNotBlank(),
                    ) {
                        Text("Update Profile")
                    }
                    OutlinedButton(
                        onClick = { launcher.launch("image/*") },
                        enabled = !isBusy,
                    ) {
                        Icon(Icons.Outlined.AddAPhoto, contentDescription = null)
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Upload Photo")
                    }
                }
            }
        }

        SectionCard {
            Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                Text(
                    text = "Password",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold,
                )
                OutlinedTextField(
                    value = currentPassword,
                    onValueChange = { currentPassword = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Current Password") },
                    visualTransformation = PasswordVisualTransformation(),
                    singleLine = true,
                )
                OutlinedTextField(
                    value = newPassword,
                    onValueChange = { newPassword = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("New Password") },
                    visualTransformation = PasswordVisualTransformation(),
                    singleLine = true,
                )
                Button(
                    onClick = {
                        onChangePassword(currentPassword, newPassword)
                        currentPassword = ""
                        newPassword = ""
                    },
                    enabled = !isBusy && currentPassword.isNotBlank() && newPassword.isNotBlank(),
                ) {
                    Text("Change Password")
                }
            }
        }

        SectionCard {
            Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
                Text(
                    text = "Account Snapshot",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold,
                )
                InfoLine("Username", profile?.username.orEmpty())
                InfoLine("Email", profile?.email.orEmpty())
                InfoLine("Contact", profile?.contactNumber.orEmpty().ifBlank { "Not set" })
                InfoLine("Assigned Area", profile?.assignedArea.orEmpty().ifBlank { "Not set" })
                InfoLine("Availability", if (profile?.isAvailable == true) "Available" else "Unavailable")
            }
        }
    }
}

@Composable
private fun TaskSummaryCard(task: TaskItemDto) {
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.Top,
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = task.title.orEmpty(),
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.SemiBold,
                    )
                    if (!task.householdName.isNullOrBlank()) {
                        Text(
                            text = task.householdName,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                }
                StatusBadge(label = task.priority.orEmpty(), tint = badgeColor(task.priority.orEmpty()))
            }
            Text(text = task.description.orEmpty())
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                StatusBadge(label = task.status.orEmpty(), tint = badgeColor(task.status.orEmpty()))
                Text(
                    text = formatApiDate(task.taskDate),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }
        }
    }
}

@Composable
private fun TaskEditorCard(
    task: TaskItemDto,
    isBusy: Boolean,
    onUpdateTaskStatus: (Int, String) -> Unit,
) {
    var selectedStatus by rememberSaveable(task.id) { mutableStateOf(task.status.orEmpty()) }

    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.Top,
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = task.title.orEmpty(),
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.SemiBold,
                    )
                    Text(text = task.description.orEmpty())
                    val location = listOfNotNull(task.householdName, task.householdAddress).joinToString(" - ")
                    if (location.isNotBlank()) {
                        Text(
                            text = location,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                }
                Column(horizontalAlignment = Alignment.End, verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    StatusBadge(label = task.priority.orEmpty(), tint = badgeColor(task.priority.orEmpty()))
                    StatusBadge(label = task.status.orEmpty(), tint = badgeColor(task.status.orEmpty()))
                }
            }
            Text(
                text = formatApiDate(task.taskDate),
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )
            DropdownField(
                label = "Task Status",
                selectedValue = selectedStatus,
                options = TASK_STATUSES.map { ChoiceItem(value = it, label = it) },
                valueLabel = { it.ifBlank { "Select status" } },
                onSelected = { selectedStatus = it.value },
            )
            Button(
                onClick = { onUpdateTaskStatus(task.id, selectedStatus) },
                enabled = !isBusy && selectedStatus.isNotBlank(),
            ) {
                Text("Update Status")
            }
        }
    }
}

@Composable
private fun HealthRecordCard(record: HealthRecordDto) {
    val resolvedAddress = record.address?.takeIf { it.isNotBlank() }
        ?: record.visitAddress?.takeIf { it.isNotBlank() }
        ?: record.householdAddress
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text(
                text = record.disease.orEmpty().ifBlank { "Undisclosed Condition" },
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
            )
            Text(text = record.symptoms.orEmpty())
            DetailText(
                values = listOfNotNull(record.patientName, record.householdName, resolvedAddress, formatApiDate(record.dateRecorded)),
            )
            DetailText(
                values = listOfNotNull(
                    record.emergencyContactName?.takeIf { it.isNotBlank() }?.let { name ->
                        if (record.emergencyContactNumber.isNullOrBlank()) name else "$name - ${record.emergencyContactNumber}"
                    },
                ),
            )
            StatusBadge(
                label = record.status.orEmpty(),
                tint = badgeColor(record.status.orEmpty()),
            )
        }
    }
}

@Composable
private fun ReportCard(report: RecentReportDto) {
    val resolvedAddress = report.address?.takeIf { it.isNotBlank() }
        ?: report.visitAddress?.takeIf { it.isNotBlank() }
        ?: report.householdAddress
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text(
                text = report.reportType.orEmpty().ifBlank { "Consultation Log" },
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
            )
            Text(text = report.content.orEmpty())
            DetailText(
                values = listOfNotNull(report.patientName, report.householdName, resolvedAddress, formatApiDate(report.dateGenerated)),
            )
            DetailText(
                values = listOfNotNull(
                    report.emergencyContactName?.takeIf { it.isNotBlank() }?.let { name ->
                        if (report.emergencyContactNumber.isNullOrBlank()) name else "$name - ${report.emergencyContactNumber}"
                    },
                ),
            )
        }
    }
}

@Composable
private fun ConsultationLogCard(log: ConsultationLogDto) {
    val resolvedAddress = log.address?.takeIf { it.isNotBlank() }
        ?: log.visitAddress?.takeIf { it.isNotBlank() }
        ?: log.householdAddress
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text(
                text = log.reportType.orEmpty().ifBlank { "Consultation Log" },
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
            )
            Text(text = log.content.orEmpty())
            DetailText(
                values = listOfNotNull(log.patientName, log.householdName, resolvedAddress, log.dateSubmitted),
            )
            DetailText(
                values = listOfNotNull(
                    log.emergencyContactName?.takeIf { it.isNotBlank() }?.let { name ->
                        if (log.emergencyContactNumber.isNullOrBlank()) name else "$name - ${log.emergencyContactNumber}"
                    },
                ),
            )
        }
    }
}

@Composable
private fun InsightCard(insight: InsightDto) {
    SectionCard {
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Text(
                    text = insight.title.orEmpty(),
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold,
                    modifier = Modifier.weight(1f),
                )
                StatusBadge(
                    label = insight.severity.orEmpty(),
                    tint = badgeColor(insight.severity.orEmpty()),
                )
            }
            Text(text = insight.description.orEmpty())
        }
    }
}

@Composable
private fun MetricCard(
    modifier: Modifier = Modifier,
    label: String,
    value: String,
    supporting: String,
    tint: Color,
) {
    Card(
        modifier = modifier,
        colors = CardDefaults.cardColors(containerColor = tint.copy(alpha = 0.12f)),
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
        ) {
            Text(text = label, color = tint, fontWeight = FontWeight.SemiBold)
            Text(text = value, style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
            Text(text = supporting, color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
    }
}

@Composable
private fun SectionHeading(
    title: String,
    subtitle: String,
) {
    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
        Text(text = title, style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.SemiBold)
        if (subtitle.isNotBlank()) {
            Text(text = subtitle, color = MaterialTheme.colorScheme.onSurfaceVariant)
        }
    }
}

@Composable
private fun EmptyStateCard(message: String) {
    SectionCard {
        Text(text = message, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

@Composable
private fun DetailText(values: List<String?>) {
    val content = values
        .filterNotNull()
        .filter { it.isNotBlank() }
        .joinToString(" - ")

    if (content.isNotBlank()) {
        Text(
            text = content,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )
    }
}

@Composable
private fun SectionCard(content: @Composable ColumnScope.() -> Unit) {
    OutlinedCard(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(24.dp),
    ) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp),
            content = content,
        )
    }
}

@Composable
private fun StatusBadge(label: String, tint: Color) {
    Surface(
        shape = CircleShape,
        color = tint.copy(alpha = 0.16f),
    ) {
        Text(
            text = label.ifBlank { "Unknown" },
            modifier = Modifier.padding(horizontal = 12.dp, vertical = 6.dp),
            color = tint,
            style = MaterialTheme.typography.labelMedium,
        )
    }
}

@Composable
private fun InfoLine(label: String, value: String) {
    Column(verticalArrangement = Arrangement.spacedBy(2.dp)) {
        Text(
            text = label,
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )
        Text(text = value)
        HorizontalDivider()
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun DropdownField(
    label: String,
    selectedValue: String,
    options: List<ChoiceItem>,
    valueLabel: (String) -> String,
    onSelected: (ChoiceItem) -> Unit,
) {
    var expanded by remember { mutableStateOf(false) }

    ExposedDropdownMenuBox(
        expanded = expanded,
        onExpandedChange = { expanded = !expanded },
    ) {
        OutlinedTextField(
            value = valueLabel(selectedValue),
            onValueChange = {},
            modifier = Modifier.fillMaxWidth().menuAnchor(),
            label = { Text(label) },
            readOnly = true,
            trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = expanded) },
        )
        ExposedDropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false },
        ) {
            options.forEach { item ->
                DropdownMenuItem(
                    text = { Text(item.label) },
                    onClick = {
                        onSelected(item)
                        expanded = false
                    },
                )
            }
        }
    }
}

private fun badgeColor(value: String): Color {
    return when (value.lowercase(Locale.getDefault())) {
        "high", "urgent", "escalated" -> Color(0xFFDC2626)
        "medium", "started", "ongoing", "monitoring" -> Color(0xFFD97706)
        "done", "completed", "available", "submitted", "info" -> Color(0xFF0F766E)
        "low" -> Color(0xFF2563EB)
        else -> Color(0xFF475569)
    }
}

private fun isToday(value: String?): Boolean {
    return parseApiDateTime(value)?.toLocalDate() == LocalDate.now()
}

private fun formatApiDate(value: String?): String {
    val formatter = DateTimeFormatter.ofPattern("MMM d, yyyy h:mm a")
    return parseApiDateTime(value)?.format(formatter) ?: value.orEmpty()
}

private fun parseApiDateTime(value: String?): java.time.ZonedDateTime? {
    if (value.isNullOrBlank()) {
        return null
    }

    return runCatching {
        OffsetDateTime.parse(value).atZoneSameInstant(ZoneId.systemDefault())
    }.recoverCatching {
        Instant.parse(value).atZone(ZoneId.systemDefault())
    }.recoverCatching {
        LocalDateTime.parse(value).atZone(ZoneId.systemDefault())
    }.getOrNull()
}

private fun resolveImageUrl(path: String?, apiBaseUrl: String): String? {
    if (path.isNullOrBlank()) {
        return null
    }

    return try {
        if (path.startsWith("http://") || path.startsWith("https://")) {
            path
        } else {
            "${apiBaseUrl.trimEnd('/')}/${path.trimStart('/')}"
        }
    } catch (e: Exception) {
        null
    }
}

private fun android.content.Context.encodeUriToBase64(uri: Uri): String? {
    return try {
        contentResolver.openInputStream(uri)?.use { stream ->
            val bytes = stream.readBytes()
            Base64.encodeToString(bytes, Base64.NO_WRAP)
        }
    } catch (e: Exception) {
        null
    }
}

private val TASK_STATUSES = listOf("Pending", "Started", "Ongoing", "Done")
