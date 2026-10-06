using ShopApi.Constants;

namespace ShopApi.Rules;

public static class OrderStatusRules
{
    private static readonly Dictionary<
        string,
        HashSet<string>
    > AllowedTransitions =
        new()
        {
            [OrderStatuses.Pending] =
                new HashSet<string>
                {
                    OrderStatuses.Paid,
                    OrderStatuses.Cancelled
                },

            [OrderStatuses.Paid] =
                new HashSet<string>
                {
                    OrderStatuses.Preparing,
                    OrderStatuses.Cancelled
                },

            [OrderStatuses.Preparing] =
                new HashSet<string>
                {
                    OrderStatuses.Shipped,
                    OrderStatuses.Cancelled
                },

            [OrderStatuses.Shipped] =
                new HashSet<string>
                {
                    OrderStatuses.Delivered
                },

            [OrderStatuses.Delivered] =
                new HashSet<string>(),

            [OrderStatuses.Cancelled] =
                new HashSet<string>()
        };

    public static bool CanTransition(
        string currentStatus,
        string newStatus
    )
    {
        if (
            !AllowedTransitions.TryGetValue(
                currentStatus,
                out var allowedStatuses
            )
        )
        {
            return false;
        }

        return allowedStatuses.Contains(
            newStatus
        );
    }

    public static bool IsValidStatus(
        string status
    )
    {
        return AllowedTransitions
            .ContainsKey(status);
    }
}