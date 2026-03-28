using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatApp : PhoneApp
{
    private ScrollView chatHistory;
    private VisualElement currentChoiceContainer;

    private int notifCount = 0;

    public override void Initialize(VisualElement root, PhoneOS os)
    {
        base.Initialize(root, os);
        ScreenContainer = root.Q<VisualElement>("ChatScreen");
        chatHistory = root.Q<ScrollView>("ChatHistory");

        // Wire up the Back button to return to Home
        Button btnBack = root.Q<Button>("Btn_Back_Chat");
        btnBack?.RegisterCallback<ClickEvent>(ev => OS.OpenApp<HomeApp>());
    }

    public void ReceiveMessage(string senderName, string messageBody, Color nameColor)
    {
        if (chatHistory == null) return;

        VisualElement container = new VisualElement();
        container.AddToClassList("chat-bubble-container");

        VisualElement bubble = new VisualElement();
        bubble.AddToClassList("chat-bubble");

        if (senderName == "Me")
        {
            container.AddToClassList("me");
            bubble.AddToClassList("me");
        }
        else
        {
            Label senderLabel = new Label(senderName);
            senderLabel.AddToClassList("chat-sender-name");
            senderLabel.style.color = new StyleColor(nameColor);
            bubble.Add(senderLabel);
        }

        Label bodyLabel = new Label(messageBody);
        bodyLabel.AddToClassList("chat-message-text");
        bubble.Add(bodyLabel);
        container.Add(bubble);

        chatHistory.Add(container);
        notifCount += 1;
        if (!isOpen) PhoneOS.Instance.GetApp<HomeApp>().UpdateChatNotification(notifCount);

        chatHistory.schedule.Execute(() =>
        {
            chatHistory.scrollOffset = new Vector2(0, chatHistory.contentContainer.layout.height);
        }).ExecuteLater(50);
    }

    public override void OnClose()
    {
        base.OnClose();
        notifCount = 0;
        PhoneOS.Instance.GetApp<HomeApp>().UpdateChatNotification(notifCount);
    }

    public void ShowChoices(List<ChoiceSaveData> choices, System.Action<string> onChoiceMade)
    {
        if (ScreenContainer == null) return;

        currentChoiceContainer = new VisualElement();
        currentChoiceContainer.AddToClassList("chat-choice-container");

        foreach (var choice in choices)
        {
            Button btn = new Button { text = choice.ChoiceText };
            btn.AddToClassList("chat-choice-btn");

            string capturedPortID = choice.PortID;
            string capturedText = choice.ChoiceText;

            btn.clicked += () =>
            {
                currentChoiceContainer.RemoveFromHierarchy();
                currentChoiceContainer = null;
                ReceiveMessage("Me", capturedText, new Color(0.8f, 0.8f, 0.8f));
                onChoiceMade?.Invoke(capturedPortID);
            };

            currentChoiceContainer.Add(btn);
        }

        ScreenContainer.Insert(2, currentChoiceContainer);

        chatHistory.schedule.Execute(() =>
        {
            chatHistory.scrollOffset = new Vector2(0, chatHistory.contentContainer.layout.height);
        }).ExecuteLater(50);
    }
}