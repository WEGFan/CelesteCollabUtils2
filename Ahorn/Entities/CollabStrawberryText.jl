module CollabUtils2StrawberryCounter

using ..Ahorn, Maple

@mapdef Entity "CollabUtils22020/strawberryCounter" CollabUtils2StrawbCounter(x::Integer, y::Integer, author::String="JaThePlayer", maxBerries::Integer=2)

const placements = Ahorn.PlacementDict(
    "Strawberry Text (Collab Utils 2)" => Ahorn.EntityPlacement(
        CollabUtils2StrawbCounter
    )
)

function Ahorn.selection(entity::CollabUtils2StrawbCounter)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CollabUtils2StrawbCounter, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.speechBubble, -12, -12)

end