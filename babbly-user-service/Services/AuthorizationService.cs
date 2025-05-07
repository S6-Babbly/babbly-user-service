using babbly_user_service.Models;

namespace babbly_user_service.Services
{
    public class AuthorizationService
    {
        private readonly KafkaProducerService _producerService;
        private readonly KafkaConsumerService _consumerService;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public AuthorizationService(
            KafkaProducerService producerService,
            KafkaConsumerService consumerService,
            ILogger<AuthorizationService> logger)
        {
            _producerService = producerService;
            _consumerService = consumerService;
            _logger = logger;
        }

        /// <summary>
        /// Check if a user is authorized to perform an operation on a resource
        /// </summary>
        public async Task<bool> IsAuthorizedAsync(string userId, List<string> roles, string resourcePath, string operation)
        {
            try
            {
                _logger.LogInformation("Requesting authorization for user {userId} on resource {resourcePath} for operation {operation}",
                    userId, resourcePath, operation);

                // Send authorization request and get correlation ID
                string correlationId = await _producerService.RequestAuthorizationAsync(userId, roles, resourcePath, operation);
                
                // Wait for response with timeout
                var response = await _consumerService.WaitForAuthorizationResponseAsync(correlationId, _defaultTimeout);

                _logger.LogInformation("Received authorization response: {isAuthorized}", response.IsAuthorized);
                return response.IsAuthorized;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Authorization request timed out");
                return false; // Fail closed - if authorization times out, deny access
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authorization check");
                return false; // Fail closed - if there's an error, deny access
            }
        }
    }
} 